using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MicroserviceFramework.Domain;
using MicroserviceFramework.Ef;
using MicroserviceFramework.Ef.Repositories;
using MicroserviceFramework.Extensions.DependencyInjection;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Xunit;

namespace MSFramework.Tests;

/// <summary>
/// 无键仓储（方案 B）测试：
/// T8 IRepository&lt;TAggregateRoot&gt; 接口 + EfRepository&lt;TAggregateRoot&gt; 基类，
/// 面向以多个标量属性直接作主键（无 Id 包装、实现非泛型 IAggregateRoot）的复合主键聚合根。
/// </summary>
public class KeylessRepositoryTests
{
    /// <summary>
    /// 多属性复合主键聚合根：以 OrderId + ProductId 直接作主键，无 Id 包装
    /// </summary>
    public class OrderLine : EntityBase, IAggregateRoot, IDeletion
    {
        protected OrderLine()
        {
        }

        public OrderLine(string orderId, string productId, string name) : this()
        {
            OrderId = orderId;
            ProductId = productId;
            Name = name;
        }

        /// <summary>
        /// 订单号（复合主键成员 1）
        /// </summary>
        public string OrderId { get; private set; }

        /// <summary>
        /// 产品号（复合主键成员 2）
        /// </summary>
        public string ProductId { get; private set; }

        /// <summary>
        /// 名称
        /// </summary>
        public string Name { get; private set; }

        /// <inheritdoc/>
        public bool IsDeleted { get; private set; }

        /// <inheritdoc/>
        public string DeleterId { get; private set; }

        /// <inheritdoc/>
        public string DeleterName { get; private set; }

        /// <inheritdoc/>
        public DateTimeOffset? DeletionTime { get; private set; }

        /// <inheritdoc/>
        public void SetDeletion(string deleterId, string deleterName, DateTimeOffset? deletionTime = null)
        {
            IsDeleted = true;
            DeleterId = deleterId;
            DeleterName = deleterName;
            DeletionTime = deletionTime ?? DateTimeOffset.UtcNow;
        }
    }

    /// <summary>
    /// 测试用 DbContext，使用顶层标量多列 HasKey 配置复合主键实体
    /// </summary>
    public class KeylessTestContext(DbContextOptions<KeylessTestContext> options) : DbContextBase(options)
    {
        public DbSet<OrderLine> OrderLines => Set<OrderLine>();

        protected override void ApplyConfiguration(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<OrderLine>(builder =>
            {
                builder.HasKey(x => new { x.OrderId, x.ProductId });
                builder.Property(x => x.OrderId).HasMaxLength(36).IsRequired();
                builder.Property(x => x.ProductId).HasMaxLength(36).IsRequired();
            });
        }
    }

    /// <summary>
    /// 测试用实体配置查找器，将实体映射到测试 DbContext
    /// </summary>
    private sealed class TestEntityConfigurationTypeFinder : IEntityConfigurationTypeFinder
    {
        private static readonly Type ContextType = typeof(KeylessTestContext);

        public IEnumerable<IEntityTypeConfiguration> GetEntityTypeConfigurations(Type dbContextType) => [];

        public Type GetDbContextTypeForEntity(Type entityType) =>
            entityType == typeof(OrderLine) ? ContextType : null;

        public IEnumerable<Type> GetAllDbContextTypes() => [ContextType];

        public bool HasDbContextForEntity<T>() => typeof(T) == typeof(OrderLine);
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
    /// 构建测试服务容器：注册 DbContext（SQLite 内存库）、设置、实体查找器与作用域提供程序
    /// </summary>
    private static TestHost CreateHost()
    {
        var connection = new SqliteConnection("DataSource=:memory:");
        connection.Open();

        var services = new ServiceCollection();
        services.AddSingleton(new DbContextSettings { UseUnderScoreCase = true, DatabaseType = "Sqlite" });
        services.AddSingleton<IEntityConfigurationTypeFinder>(new TestEntityConfigurationTypeFinder());
        services.AddDbContext<KeylessTestContext>(options => options.UseSqlite(connection));
        services.TryAddScoped<DbContextFactory>();
        // 与 AddEntityFrameworkExtension 的开放泛型注册一致：
        // typeof(IRepository<>) -> typeof(EfRepository<>)
        services.TryAddScoped(typeof(IRepository<>), typeof(EfRepository<>));

        var rootProvider = services.BuildServiceProvider();
        services.AddSingleton<IScopeServiceProvider>(new TestScopeServiceProvider(rootProvider));
        var provider = services.BuildServiceProvider();
        provider.GetRequiredService<KeylessTestContext>().Database.EnsureCreated();

        return new TestHost(provider, connection);
    }

    /// <summary>
    /// 创建无键仓储实例
    /// </summary>
    private static EfRepository<OrderLine> CreateRepository(ServiceProvider provider)
    {
        return new EfRepository<OrderLine>(new DbContextFactory(provider));
    }

    [Fact]
    public async Task GetQueryable_FiltersByPredicate()
    {
        using var host = CreateHost();
        using var context = host.Provider.GetRequiredService<KeylessTestContext>();
        var repository = CreateRepository(host.Provider);

        repository.Add(new OrderLine("O1", "P1", "apple"));
        repository.Add(new OrderLine("O1", "P2", "banana"));
        await context.SaveChangesAsync();

        var result = repository.GetQueryable().Where(x => x.OrderId == "O1" && x.ProductId == "P1").Single();
        Assert.Equal("apple", result.Name);
    }

    [Fact]
    public async Task GetQueryableAsync_ReturnsQueryable()
    {
        using var host = CreateHost();
        using var context = host.Provider.GetRequiredService<KeylessTestContext>();
        var repository = CreateRepository(host.Provider);

        repository.Add(new OrderLine("O1", "P1", "apple"));
        await context.SaveChangesAsync();

        var queryable = await repository.GetQueryableAsync();
        Assert.Single(queryable);
    }

    [Fact]
    public async Task FindAsync_ByMemberPredicate_ReturnsEntity()
    {
        using var host = CreateHost();
        using var context = host.Provider.GetRequiredService<KeylessTestContext>();
        var repository = CreateRepository(host.Provider);

        repository.Add(new OrderLine("O1", "P1", "apple"));
        await context.SaveChangesAsync();

        var found = await repository.FindAsync(x => x.OrderId == "O1" && x.ProductId == "P1");
        Assert.NotNull(found);
        Assert.Equal("apple", found.Name);

        var notFound = await repository.FindAsync(x => x.OrderId == "O2" && x.ProductId == "P1");
        Assert.Null(notFound);
    }

    [Fact]
    public async Task FindAsync_SoftDeletedEntity_ReturnsNull()
    {
        using var host = CreateHost();
        using var context = host.Provider.GetRequiredService<KeylessTestContext>();
        var repository = CreateRepository(host.Provider);

        var entity = new OrderLine("O1", "P1", "apple");
        repository.Add(entity);
        await context.SaveChangesAsync();

        entity.SetDeletion("u1", "n1");
        await context.SaveChangesAsync();

        Assert.True(entity.IsDeleted);
        Assert.Null(await repository.FindAsync(x => x.OrderId == "O1" && x.ProductId == "P1"));
    }

    [Fact]
    public async Task Delete_RemovesEntity()
    {
        using var host = CreateHost();
        using var context = host.Provider.GetRequiredService<KeylessTestContext>();
        var repository = CreateRepository(host.Provider);

        var entity = new OrderLine("O1", "P1", "apple");
        repository.Add(entity);
        await context.SaveChangesAsync();

        repository.Delete(entity);
        await context.SaveChangesAsync();

        Assert.Empty(context.OrderLines);
    }

    [Fact]
    public async Task DeleteAsync_RemovesEntity()
    {
        using var host = CreateHost();
        using var context = host.Provider.GetRequiredService<KeylessTestContext>();
        var repository = CreateRepository(host.Provider);

        var entity = new OrderLine("O1", "P1", "apple");
        repository.Add(entity);
        await context.SaveChangesAsync();

        await repository.DeleteAsync(entity);
        await context.SaveChangesAsync();

        Assert.Empty(context.OrderLines);
    }

    [Fact]
    public async Task AddAsync_ThenFind_ReturnsEntity()
    {
        using var host = CreateHost();
        using var context = host.Provider.GetRequiredService<KeylessTestContext>();
        var repository = CreateRepository(host.Provider);

        await repository.AddAsync(new OrderLine("O1", "P1", "apple"));
        await context.SaveChangesAsync();

        var found = await repository.FindAsync(x => x.OrderId == "O1" && x.ProductId == "P1");
        Assert.NotNull(found);
    }

    [Fact]
    public void MultiColumnKey_HasTwoPrimaryKeyProperties()
    {
        using var host = CreateHost();
        using var context = host.Provider.GetRequiredService<KeylessTestContext>();
        context.Database.EnsureCreated();

        var entityType = context.Model.FindEntityType(typeof(OrderLine));
        var keyProperties = entityType.FindPrimaryKey().Properties;
        Assert.Equal(2, keyProperties.Count);
        Assert.Contains(keyProperties, p => p.Name == "OrderId");
        Assert.Contains(keyProperties, p => p.Name == "ProductId");
    }

    [Fact]
    public void GetAuditEntities_MultiColumnKey_JoinsKeyValues()
    {
        using var host = CreateHost();
        using var context = host.Provider.GetRequiredService<KeylessTestContext>();

        context.OrderLines.Add(new OrderLine("O1", "P1", "apple"));

        var auditEntity = context.GetAuditEntities().Single(x => x.Type.Contains(nameof(OrderLine)));
        Assert.Equal("O1|P1", auditEntity.EntityId);
    }

    [Fact]
    public void OpenGenericRegistration_ResolvesKeylessRepository()
    {
        using var host = CreateHost();

        // 与 AddEntityFrameworkExtension 的开放泛型注册一致：
        // typeof(IRepository<>) -> typeof(EfRepository<>)
        using var scope = host.Provider.CreateScope();
        var repository = scope.ServiceProvider.GetRequiredService<IRepository<OrderLine>>();

        Assert.IsType<EfRepository<OrderLine>>(repository);
        Assert.NotNull(repository.GetQueryable());
    }
}
