using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MicroserviceFramework;
using MicroserviceFramework.Auditing;
using MicroserviceFramework.Auditing.Model;
using MicroserviceFramework.Domain;
using MicroserviceFramework.Ef;
using MicroserviceFramework.Extensions.DependencyInjection;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Xunit;

namespace MSFramework.Tests;

/// <summary>
/// <see cref="EfUnitOfWork"/> 从 <see cref="AuditOperationContext"/>（AsyncLocal）读取审计操作的集成测试：
/// 验证保存回调只收集当前执行流承载的审计操作（OnSavingChanges 从 AsyncLocal 读取），
/// 未设置审计操作时跳过收集；实体经 <see cref="AuditOperation.AddEntities"/> 防重后进入操作。
/// </summary>
public class AuditOperationAsyncLocalFlowTests
{
    /// <summary>
    /// 测试用审计实体（string 主键聚合根）
    /// </summary>
    public class AuditFlowOrder : DeletionAggregateRoot<string>
    {
        protected AuditFlowOrder() : base(default!)
        {
        }

        public AuditFlowOrder(string id, string name) : base(id)
        {
            Name = name;
        }

        /// <summary>
        /// 名称
        /// </summary>
        public string Name { get; set; }
    }

    /// <summary>
    /// 测试用 DbContext
    /// </summary>
    public class AuditFlowContext(DbContextOptions<AuditFlowContext> options) : DbContextBase(options)
    {
        public DbSet<AuditFlowOrder> Orders => Set<AuditFlowOrder>();

        protected override void ApplyConfiguration(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<AuditFlowOrder>(builder =>
                builder.Property(x => x.Id).HasMaxLength(36).IsRequired());
        }
    }

    /// <summary>
    /// 测试用实体配置查找器，将实体映射到测试 DbContext
    /// </summary>
    private sealed class TestEntityConfigurationTypeFinder : IEntityConfigurationTypeFinder
    {
        private static readonly Dictionary<Type, Type> EntityToDbContext = new()
        {
            [typeof(AuditFlowOrder)] = typeof(AuditFlowContext)
        };

        public IEnumerable<IEntityTypeConfiguration> GetEntityTypeConfigurations(Type dbContextType) => [];

        public Type GetDbContextTypeForEntity(Type entityType) =>
            EntityToDbContext.TryGetValue(entityType, out var contextType) ? contextType : null;

        public IEnumerable<Type> GetAllDbContextTypes() =>
            EntityToDbContext.Values.Distinct();

        public bool HasDbContextForEntity<T>() => EntityToDbContext.ContainsKey(typeof(T));
    }

    /// <summary>
    /// 测试用作用域服务提供程序
    /// </summary>
    private sealed class TestScopeServiceProvider(IServiceProvider serviceProvider) : IScopeServiceProvider
    {
        public T GetService<T>() => serviceProvider.GetService<T>();
    }

    /// <summary>
    /// 测试宿主：持有 SQLite 内存连接与服务容器，保证连接在测试生命周期内存活
    /// </summary>
    private sealed class TestHost : IDisposable
    {
        public TestHost(ServiceProvider provider, SqliteConnection connection)
        {
            Provider = provider;
            _connection = connection;
        }

        public ServiceProvider Provider { get; }

        private readonly SqliteConnection _connection;

        public void Dispose()
        {
            _connection.Dispose();
            Provider.Dispose();
        }
    }

    /// <summary>
    /// 构建测试服务容器：注册 DbContext（SQLite 内存库）、EF 扩展（IUnitOfWork→EfUnitOfWork）、
    /// 实体查找器与作用域提供程序
    /// </summary>
    /// <returns>测试宿主</returns>
    private static TestHost CreateHost()
    {
        var connection = new SqliteConnection("DataSource=:memory:");
        connection.Open();

        var services = new ServiceCollection();
        // 初始化框架（程序集扫描），AddEntityFrameworkExtension 依赖 Utils.Runtime 类型缓存
        services.AddMicroserviceFramework();
        services.AddSingleton(new DbContextSettings { UseUnderScoreCase = true, DatabaseType = "Sqlite" });
        services.AddSingleton<IEntityConfigurationTypeFinder>(new TestEntityConfigurationTypeFinder());
        services.AddDbContext<AuditFlowContext>(options => options.UseSqlite(connection));

        var rootProvider = services.BuildServiceProvider();
        services.AddSingleton<IScopeServiceProvider>(new TestScopeServiceProvider(rootProvider));

        // 注册 IUnitOfWork→EfUnitOfWork、DbContextFactory 等 EF 扩展服务
        services.AddEntityFrameworkExtension();

        var provider = services.BuildServiceProvider();
        provider.GetRequiredService<AuditFlowContext>().Database.EnsureCreated();

        return new TestHost(provider, connection);
    }

    /// <summary>
    /// 构建测试用审计操作
    /// </summary>
    /// <returns>审计操作实例</returns>
    private static AuditOperation CreateOperation()
    {
        return new AuditOperation("/orders", "ua", "1.2.3.4", "iPhone", "device-1",
            null, null, "trace-1", "POST");
    }

    /// <summary>
    /// 完整链路：注册审计操作并设置 AsyncLocal 后，SaveChanges 回调必须从执行流读取到审计操作并收集变更实体
    /// </summary>
    [Fact]
    public async Task SaveChanges_WithAsyncLocalOperation_CollectsEntities()
    {
        using var host = CreateHost();
        using var scope = host.Provider.CreateScope();
        var unitOfWork = scope.ServiceProvider.GetRequiredService<IUnitOfWork>();
        var context = scope.ServiceProvider.GetRequiredService<AuditFlowContext>();

        var operation = CreateOperation();
        unitOfWork.RegisterAuditOperation(operation);
        AuditOperationContext.Value = operation;
        try
        {
            context.Orders.Add(new AuditFlowOrder("ORD-1", "order-1"));
            await unitOfWork.SaveChangesAsync();

            var entity = Assert.Single(operation.Entities);
            Assert.Equal("ORD-1", entity.EntityId);
            Assert.Equal(OperationType.Add, entity.OperationType);
            Assert.Contains(nameof(AuditFlowOrder), entity.Type);
            Assert.Same(operation, entity.Operation);
        }
        finally
        {
            AuditOperationContext.Value = null;
        }
    }

    /// <summary>
    /// 未设置 AsyncLocal 审计操作时，即使已注册订阅，保存回调也必须跳过收集（不误收集）
    /// </summary>
    [Fact]
    public async Task SaveChanges_WithoutAsyncLocalOperation_SkipsCollection()
    {
        using var host = CreateHost();
        using var scope = host.Provider.CreateScope();
        var unitOfWork = scope.ServiceProvider.GetRequiredService<IUnitOfWork>();
        var context = scope.ServiceProvider.GetRequiredService<AuditFlowContext>();

        var operation = CreateOperation();
        unitOfWork.RegisterAuditOperation(operation);
        try
        {
            context.Orders.Add(new AuditFlowOrder("ORD-2", "order-2"));
            await unitOfWork.SaveChangesAsync();

            Assert.Empty(operation.Entities);
        }
        finally
        {
            AuditOperationContext.Value = null;
        }
    }

    /// <summary>
    /// 同一请求执行流内重复保存（残留处理器重复触发收集场景）时，同一实体的同一变更状态只收集一次
    /// </summary>
    [Fact]
    public async Task SaveChanges_RepeatedSave_CollectsEachEntityStateOnce()
    {
        using var host = CreateHost();
        using var scope = host.Provider.CreateScope();
        var unitOfWork = scope.ServiceProvider.GetRequiredService<IUnitOfWork>();
        var context = scope.ServiceProvider.GetRequiredService<AuditFlowContext>();

        var operation = CreateOperation();
        unitOfWork.RegisterAuditOperation(operation);
        AuditOperationContext.Value = operation;
        try
        {
            context.Orders.Add(new AuditFlowOrder("ORD-3", "order-3"));
            // 第一次保存：收集 Added 状态
            await unitOfWork.SaveChangesAsync();
            var addEntities = operation.Entities.Count(x => x.OperationType == OperationType.Add);

            // 同一请求内再次保存：此时无新变更状态，残留触发也不应产生重复收集
            await unitOfWork.SaveChangesAsync();
            var totalAfterSecondSave = operation.Entities.Count;

            Assert.Equal(1, addEntities);
            Assert.Equal(1, totalAfterSecondSave);
        }
        finally
        {
            AuditOperationContext.Value = null;
        }
    }
}
