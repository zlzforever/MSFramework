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
using Xunit;

namespace MSFramework.Tests;

/// <summary>
/// Stage 5 复测回归用例（ZZO-87）：
/// 验证 ZZO-86 修复后，单键 <see cref="EfRepository{TEntity,TKey}.Find(TKey)"/> /
/// <see cref="EfRepository{TEntity,TKey}.FindAsync(TKey)"/> 对带 HasMany 导航的单键聚合根
/// 恢复首级导航加载（单键行为等价），且 <c>Find(null)</c>/<c>FindAsync(null)</c> 返回 null 不抛异常；
/// 同时复测软删除守卫（已跟踪已持久化软删除实体 Find 返回 null）。
/// </summary>
public class FindNavigationRegressionTests
{
    /// <summary>
    /// 带 HasMany 导航的单键聚合根
    /// </summary>
    public class Order : DeletionAggregateRoot<string>
    {
        private readonly List<OrderItem> _items = [];

        /// <summary>
        /// 订单项集合（第一级导航属性）
        /// </summary>
        public IReadOnlyCollection<OrderItem> Items => _items;

        /// <summary>
        /// 仅供 EF Core 物化使用
        /// </summary>
        protected Order() : base(default!)
        {
        }

        /// <summary>
        /// 初始化订单聚合根
        /// </summary>
        /// <param name="id">订单主键</param>
        public Order(string id) : base(id)
        {
        }

        /// <summary>
        /// 添加订单项
        /// </summary>
        /// <param name="name">订单项名称</param>
        public void AddItem(string name)
        {
            _items.Add(new OrderItem(Guid.NewGuid().ToString(), this, name));
        }
    }

    /// <summary>
    /// 订单项实体
    /// </summary>
    public class OrderItem : EntityBase<string>
    {
        /// <summary>
        /// 所属订单（反向导航）
        /// </summary>
        public Order Order { get; private set; }

        /// <summary>
        /// 订单项名称
        /// </summary>
        public string Name { get; private set; }

        /// <summary>
        /// 仅供 EF Core 物化使用
        /// </summary>
        protected OrderItem() : base(default!)
        {
        }

        /// <summary>
        /// 初始化订单项
        /// </summary>
        /// <param name="id">订单项主键</param>
        /// <param name="order">所属订单</param>
        /// <param name="name">订单项名称</param>
        public OrderItem(string id, Order order, string name) : base(id)
        {
            Order = order;
            Name = name;
        }
    }

    /// <summary>
    /// 测试用 DbContext，配置带 HasMany 导航的单键聚合根
    /// </summary>
    public class FindRegressionContext(DbContextOptions<FindRegressionContext> options) : DbContextBase(options)
    {
        public DbSet<Order> Orders => Set<Order>();

        protected override void ApplyConfiguration(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Order>(builder =>
            {
                builder.Property(x => x.Id).HasMaxLength(36).IsRequired();
                builder.HasMany(x => x.Items).WithOne(x => x.Order);
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
            [typeof(Order)] = typeof(FindRegressionContext),
            [typeof(OrderItem)] = typeof(FindRegressionContext)
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
        services.AddDbContext<FindRegressionContext>(options => options.UseSqlite(connection));

        var rootProvider = services.BuildServiceProvider();
        services.AddSingleton<IScopeServiceProvider>(new TestScopeServiceProvider(rootProvider));
        var provider = services.BuildServiceProvider();
        provider.GetRequiredService<FindRegressionContext>().Database.EnsureCreated();

        return new TestHost(provider, connection);
    }

    /// <summary>
    /// 创建带 HasMany 导航的单键聚合根仓储实例（使用指定服务提供程序，独立于种子数据上下文）
    /// </summary>
    private static EfRepository<Order, string> CreateOrderRepository(IServiceProvider provider)
    {
        return new EfRepository<Order, string>(new DbContextFactory(provider));
    }

    [Fact]
    public void Find_UntrackedSingleKeyAggregate_LoadsFirstLevelNavigation()
    {
        using var host = CreateHost();

        // 种子数据：保存订单 + 1 个订单项
        using (var context = host.Provider.GetRequiredService<FindRegressionContext>())
        {
            var order = new Order("O-1");
            order.AddItem("apple");
            context.Orders.Add(order);
            context.SaveChanges();
        }

        // 新建作用域上下文（未跟踪任何实体）→ Find 应恢复首级导航加载
        using var scope = host.Provider.CreateScope();
        var repository = CreateOrderRepository(scope.ServiceProvider);
        var found = repository.Find("O-1");

        Assert.NotNull(found);
        Assert.Single(found.Items);
    }

    [Fact]
    public async Task FindAsync_UntrackedSingleKeyAggregate_LoadsFirstLevelNavigation()
    {
        using var host = CreateHost();

        // 种子数据：保存订单 + 1 个订单项
        using (var context = host.Provider.GetRequiredService<FindRegressionContext>())
        {
            var order = new Order("O-1");
            order.AddItem("apple");
            context.Orders.Add(order);
            await context.SaveChangesAsync();
        }

        // 新建作用域上下文（未跟踪任何实体）→ FindAsync 应恢复首级导航加载
        using var scope = host.Provider.CreateScope();
        var repository = CreateOrderRepository(scope.ServiceProvider);
        var found = await repository.FindAsync("O-1");

        Assert.NotNull(found);
        Assert.Single(found.Items);
    }

    [Fact]
    public void Find_Null_ReturnsNull_WithoutThrowing()
    {
        using var host = CreateHost();
        using var context = host.Provider.GetRequiredService<FindRegressionContext>();

        var order = new Order("O-1");
        context.Orders.Add(order);
        context.SaveChanges();

        var repository = CreateOrderRepository(host.Provider);

        Assert.Null(repository.Find(null));
    }

    [Fact]
    public async Task FindAsync_Null_ReturnsNull_WithoutThrowing()
    {
        using var host = CreateHost();
        using var context = host.Provider.GetRequiredService<FindRegressionContext>();

        var order = new Order("O-1");
        context.Orders.Add(order);
        await context.SaveChangesAsync();

        var repository = CreateOrderRepository(host.Provider);

        Assert.Null(await repository.FindAsync(null));
    }

    [Fact]
    public void Find_TrackedPersistedSoftDeletedEntity_ReturnsNull()
    {
        using var host = CreateHost();
        using var context = host.Provider.GetRequiredService<FindRegressionContext>();
        var repository = CreateOrderRepository(host.Provider);

        var order = new Order("O-1");
        repository.Add(order);
        context.SaveChanges();

        // 软删除后实体仍被 ChangeTracker 跟踪（模拟框架软删除策略后的状态）
        order.SetDeletion("u1", "n1");
        context.SaveChanges();

        Assert.True(order.IsDeleted);
        Assert.Null(repository.Find("O-1"));
    }
}
