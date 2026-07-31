using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MicroserviceFramework.Domain;
using MicroserviceFramework.Ef;
using MicroserviceFramework.Ef.Extensions;
using MicroserviceFramework.Ef.Repositories;
using MicroserviceFramework.Extensions.DependencyInjection;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Xunit;

namespace MSFramework.Tests;

/// <summary>
/// 复合主键集成测试：
/// T3 EfRepository 复合键安全化（Find/FindAsync/Delete 使用 DbSet.Find + 软删除守卫）
/// T4 DbContextBase.GetAuditEntity 多主键拼接 entityId
/// T5 EntityTypeBuilderExtensions.ConfigureCompositeKey 值对象键映射辅助
/// </summary>
public class CompositeKeyTests
{
    /// <summary>
    /// 复合主键值对象（record class）
    /// </summary>
    public sealed record OrderItemKey(string OrderId, string ProductId)
    {
        /// <inheritdoc/>
        public override string ToString() => $"{OrderId}|{ProductId}";

        /// <summary>
        /// 从数据库字符串还原复合键
        /// </summary>
        public static OrderItemKey Parse(string value)
        {
            var parts = value.Split('|');
            return new OrderItemKey(parts[0], parts[1]);
        }
    }

    /// <summary>
    /// 复合主键聚合根
    /// </summary>
    public class OrderItem : DeletionAggregateRoot<OrderItemKey>
    {
        protected OrderItem() : base(default!)
        {
        }

        public OrderItem(OrderItemKey id, string name) : base(id)
        {
            Name = name;
        }

        public string Name { get; set; }
    }

    /// <summary>
    /// 单主键聚合根（回归验证）
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

        public string Name { get; set; }
    }

    /// <summary>
    /// 测试用 DbContext，使用 ConfigureCompositeKey 配置复合键实体
    /// </summary>
    public class CompositeKeyTestContext(DbContextOptions<CompositeKeyTestContext> options) : DbContextBase(options)
    {
        public DbSet<OrderItem> OrderItems => Set<OrderItem>();

        public DbSet<Category> Categories => Set<Category>();

        protected override void ApplyConfiguration(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<OrderItem>(builder =>
            {
                builder.ConfigureCompositeKey(
                    x => x.Id,
                    key => key.ToString(),
                    OrderItemKey.Parse);
            });
        }
    }

    /// <summary>
    /// 测试用实体配置查找器，将实体映射到测试 DbContext
    /// </summary>
    private sealed class TestEntityConfigurationTypeFinder : IEntityConfigurationTypeFinder
    {
        private static readonly Dictionary<Type, Type> EntityToDbContext = new()
        {
            [typeof(OrderItem)] = typeof(CompositeKeyTestContext),
            [typeof(Category)] = typeof(CompositeKeyTestContext)
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
        services.AddDbContext<CompositeKeyTestContext>(options => options.UseSqlite(connection));

        var rootProvider = services.BuildServiceProvider();
        services.AddSingleton<IScopeServiceProvider>(new TestScopeServiceProvider(rootProvider));
        var provider = services.BuildServiceProvider();
        provider.GetRequiredService<CompositeKeyTestContext>().Database.EnsureCreated();

        return new TestHost(provider, connection);
    }

    /// <summary>
    /// 创建复合键实体的仓储实例
    /// </summary>
    private static EfRepository<OrderItem, OrderItemKey> CreateOrderItemRepository(ServiceProvider provider)
    {
        return new EfRepository<OrderItem, OrderItemKey>(new DbContextFactory(provider));
    }

    [Fact]
    public void Find_CompositeKey_ReturnsEntity()
    {
        using var host = CreateHost();
        using var context = host.Provider.GetRequiredService<CompositeKeyTestContext>();
        var repository = CreateOrderItemRepository(host.Provider);

        var id = new OrderItemKey("O1", "P1");
        repository.Add(new OrderItem(id, "item"));
        context.SaveChanges();

        var found = repository.Find(id);
        Assert.NotNull(found);
        Assert.Equal("item", found.Name);
    }

    [Fact]
    public async Task FindAsync_CompositeKey_ReturnsEntity()
    {
        using var host = CreateHost();
        using var context = host.Provider.GetRequiredService<CompositeKeyTestContext>();
        var repository = CreateOrderItemRepository(host.Provider);

        var id = new OrderItemKey("O1", "P1");
        await repository.AddAsync(new OrderItem(id, "item"));
        await context.SaveChangesAsync();

        var found = await repository.FindAsync(id);
        Assert.NotNull(found);
        Assert.Equal("item", found.Name);
    }

    [Fact]
    public void Delete_ByCompositeKey_RemovesEntity()
    {
        using var host = CreateHost();
        using var context = host.Provider.GetRequiredService<CompositeKeyTestContext>();
        var repository = CreateOrderItemRepository(host.Provider);

        var id = new OrderItemKey("O1", "P1");
        repository.Add(new OrderItem(id, "item"));
        context.SaveChanges();

        repository.Delete(id);
        context.SaveChanges();

        Assert.Null(repository.Find(id));
        Assert.Empty(context.OrderItems);
    }

    [Fact]
    public void Find_SoftDeletedCompositeKeyEntity_ReturnsNull()
    {
        using var host = CreateHost();
        using var context = host.Provider.GetRequiredService<CompositeKeyTestContext>();
        var repository = CreateOrderItemRepository(host.Provider);

        var id = new OrderItemKey("O1", "P1");
        var entity = new OrderItem(id, "item");
        repository.Add(entity);
        context.SaveChanges();

        // 软删除后实体仍被 ChangeTracker 跟踪（模拟框架软删除策略后的状态）
        entity.SetDeletion("u1", "n1");
        context.SaveChanges();

        Assert.True(entity.IsDeleted);
        Assert.Null(repository.Find(id));
    }

    [Fact]
    public void Delete_ByCompositeKey_SoftDeletedEntity_IsNoOp()
    {
        using var host = CreateHost();
        using var context = host.Provider.GetRequiredService<CompositeKeyTestContext>();
        var repository = CreateOrderItemRepository(host.Provider);

        var id = new OrderItemKey("O1", "P1");
        var entity = new OrderItem(id, "item");
        repository.Add(entity);
        context.SaveChanges();

        entity.SetDeletion("u1", "n1");
        context.SaveChanges();

        repository.Delete(id);
        context.SaveChanges();

        // 软删除实体不应被物理删除
        Assert.Single(context.OrderItems.IgnoreQueryFilters());
        Assert.Empty(context.OrderItems);
    }

    [Fact]
    public void GetAuditEntities_CompositeKey_JoinsKeyValues()
    {
        using var host = CreateHost();
        using var context = host.Provider.GetRequiredService<CompositeKeyTestContext>();

        context.OrderItems.Add(new OrderItem(new OrderItemKey("O1", "P1"), "item"));

        var auditEntity = context.GetAuditEntities().Single(x => x.Type.Contains(nameof(OrderItem)));
        Assert.Equal("O1|P1", auditEntity.EntityId);
    }

    [Fact]
    public void GetAuditEntities_SingleKey_EntityIdUnchanged()
    {
        using var host = CreateHost();
        using var context = host.Provider.GetRequiredService<CompositeKeyTestContext>();

        context.Categories.Add(new Category("CAT-1", "category"));

        var auditEntity = context.GetAuditEntities().Single(x => x.Type.Contains(nameof(Category)));
        Assert.Equal("CAT-1", auditEntity.EntityId);
    }

    [Fact]
    public void ConfigureCompositeKey_AppliesSnakeCaseColumnName()
    {
        using var host = CreateHost();
        using var context = host.Provider.GetRequiredService<CompositeKeyTestContext>();
        context.Database.EnsureCreated();

        var entityType = context.Model.FindEntityType(typeof(OrderItem));
        var keyProperty = entityType.FindPrimaryKey().Properties.Single();
        Assert.Equal("id", keyProperty.GetColumnName());
        Assert.Equal(typeof(OrderItemKey), keyProperty.ClrType);
    }
}
