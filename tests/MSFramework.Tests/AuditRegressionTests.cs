using System;
using System.Collections.Generic;
using System.Linq;
using MicroserviceFramework.Domain;
using MicroserviceFramework.Ef;
using MicroserviceFramework.Extensions.DependencyInjection;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Xunit;

namespace MSFramework.Tests;

/// <summary>
/// 单键审计回归测试：
/// 验证 <see cref="DbContextBase.GetAuditEntities"/> 对单主键实体拼接的
/// <see cref="AuditEntity.EntityId"/> 保持原值不拼接，即多主键 <c>|</c> 拼接逻辑
/// （<see cref="DbContextBase"/> 内部）在单键场景下行为与历史版本一致。
/// 由已移除的 <c>CompositeKeyTests.GetAuditEntities_SingleKey_EntityIdUnchanged</c> 迁移保留。
/// </summary>
public class AuditRegressionTests
{
    /// <summary>
    /// 单键聚合根（string 主键）
    /// </summary>
    public class Category : DeletionAggregateRoot<string>
    {
        protected Category() : base(default!)
        {
        }

        public Category(string id, string name) : base(id)
        {
            Name = name;
        }

        /// <summary>
        /// 名称
        /// </summary>
        public string Name { get; set; }
    }

    /// <summary>
    /// 单键聚合根（int 主键）
    /// </summary>
    public class IntIdCategory : DeletionAggregateRoot<int>
    {
        protected IntIdCategory() : base(default!)
        {
        }

        public IntIdCategory(int id, string name) : base(id)
        {
            Name = name;
        }

        /// <summary>
        /// 名称
        /// </summary>
        public string Name { get; set; }
    }

    /// <summary>
    /// 单键聚合根（Guid 主键）
    /// </summary>
    public class GuidIdCategory : DeletionAggregateRoot<Guid>
    {
        protected GuidIdCategory() : base(default!)
        {
        }

        public GuidIdCategory(Guid id, string name) : base(id)
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
    public class AuditRegressionContext(DbContextOptions<AuditRegressionContext> options) : DbContextBase(options)
    {
        public DbSet<Category> Categories => Set<Category>();

        public DbSet<IntIdCategory> IntIdCategories => Set<IntIdCategory>();

        public DbSet<GuidIdCategory> GuidIdCategories => Set<GuidIdCategory>();

        protected override void ApplyConfiguration(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Category>(builder => builder.Property(x => x.Id).HasMaxLength(36).IsRequired());
            modelBuilder.Entity<IntIdCategory>(builder => builder.Property(x => x.Id).IsRequired());
            modelBuilder.Entity<GuidIdCategory>(builder => builder.Property(x => x.Id).IsRequired());
        }
    }

    /// <summary>
    /// 测试用实体配置查找器，将实体映射到测试 DbContext
    /// </summary>
    private sealed class TestEntityConfigurationTypeFinder : IEntityConfigurationTypeFinder
    {
        private static readonly Dictionary<Type, Type> EntityToDbContext = new()
        {
            [typeof(Category)] = typeof(AuditRegressionContext),
            [typeof(IntIdCategory)] = typeof(AuditRegressionContext),
            [typeof(GuidIdCategory)] = typeof(AuditRegressionContext)
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
    /// 构建测试服务容器：注册 DbContext（SQLite 内存库）、设置、实体查找器与作用域提供程序
    /// </summary>
    private static TestHost CreateHost()
    {
        var connection = new SqliteConnection("DataSource=:memory:");
        connection.Open();

        var services = new ServiceCollection();
        services.AddSingleton(new DbContextSettings { UseUnderScoreCase = true, DatabaseType = "Sqlite" });
        services.AddSingleton<IEntityConfigurationTypeFinder>(new TestEntityConfigurationTypeFinder());
        services.AddDbContext<AuditRegressionContext>(options => options.UseSqlite(connection));

        var rootProvider = services.BuildServiceProvider();
        services.AddSingleton<IScopeServiceProvider>(new TestScopeServiceProvider(rootProvider));
        var provider = services.BuildServiceProvider();
        provider.GetRequiredService<AuditRegressionContext>().Database.EnsureCreated();

        return new TestHost(provider, connection);
    }

    [Fact]
    public void GetAuditEntities_StringSingleKey_EntityIdUnchanged()
    {
        using var host = CreateHost();
        using var context = host.Provider.GetRequiredService<AuditRegressionContext>();

        context.Categories.Add(new Category("CAT-1", "category"));

        var auditEntity = context.GetAuditEntities().Single(x => x.Type.Contains(nameof(Category)));
        Assert.Equal("CAT-1", auditEntity.EntityId);
    }

    [Fact]
    public void GetAuditEntities_IntSingleKey_EntityIdUnchanged()
    {
        using var host = CreateHost();
        using var context = host.Provider.GetRequiredService<AuditRegressionContext>();

        context.IntIdCategories.Add(new IntIdCategory(1001, "category"));

        var auditEntity = context.GetAuditEntities().Single(x => x.Type.Contains(nameof(IntIdCategory)));
        Assert.Equal("1001", auditEntity.EntityId);
    }

    [Fact]
    public void GetAuditEntities_GuidSingleKey_EntityIdUnchanged()
    {
        using var host = CreateHost();
        using var context = host.Provider.GetRequiredService<AuditRegressionContext>();

        var id = Guid.NewGuid();
        context.GuidIdCategories.Add(new GuidIdCategory(id, "category"));

        var auditEntity = context.GetAuditEntities().Single(x => x.Type.Contains(nameof(GuidIdCategory)));
        Assert.Equal(id.ToString(), auditEntity.EntityId);
    }
}
