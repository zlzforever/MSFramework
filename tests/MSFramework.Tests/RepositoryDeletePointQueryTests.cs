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
/// 仓储按主键删除回归测试：
/// 验证 <c>Delete(TKey)</c>/<c>DeleteAsync(TKey)</c> 以裸 <c>DbSet</c> 最小点查定位实体
/// （不经过 <c>Store</c>、不 Include 任何导航属性，ChangeTracker 仅含父实体），
/// 查询遵循全局查询过滤器：软删除行被隐藏视为不存在，不绕过租户隔离等其他过滤器。
/// </summary>
public class RepositoryDeletePointQueryTests
{
    /// <summary>
    /// 无审计接口的父聚合（物理删除），带集合导航（验证删除不加载导航）
    /// </summary>
    public class Order : EntityBase<string>, IAggregateRoot<string>
    {
        public Order(string id) : base(id)
        {
        }

        /// <summary>
        /// 名称
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// 子集合导航
        /// </summary>
        public List<Child> Children { get; set; } = [];
    }

    /// <summary>
    /// 子实体
    /// </summary>
    public class Child : EntityBase<int>
    {
        public Child(int id) : base(id)
        {
        }

        /// <summary>
        /// 父实体外键
        /// </summary>
        public string OrderId { get; set; }

        /// <summary>
        /// 名称
        /// </summary>
        public string Name { get; set; }
    }

    /// <summary>
    /// 软删除父聚合（验证"已软删除则跳过"检查）
    /// </summary>
    public class SoftOrder : EntityBase<string>, IAggregateRoot<string>, IDeletion
    {
        public SoftOrder(string id) : base(id)
        {
        }

        /// <summary>
        /// 名称
        /// </summary>
        public string Name { get; set; }

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
            DeletionTime = deletionTime ?? DateTimeOffset.Now;
        }
    }

    /// <summary>
    /// 测试用 DbContext
    /// </summary>
    public class DeleteTestContext(DbContextOptions<DeleteTestContext> options) : DbContextBase(options)
    {
        public DbSet<Order> Orders => Set<Order>();

        public DbSet<Child> Children => Set<Child>();

        public DbSet<SoftOrder> SoftOrders => Set<SoftOrder>();

        protected override void ApplyConfiguration(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Order>(builder =>
            {
                builder.Property(x => x.Id).HasMaxLength(36).IsRequired();
                builder.HasMany(x => x.Children).WithOne().HasForeignKey(x => x.OrderId).IsRequired();
            });
            modelBuilder.Entity<Child>(builder => builder.Property(x => x.Id).IsRequired());
            modelBuilder.Entity<SoftOrder>(builder => builder.Property(x => x.Id).HasMaxLength(36).IsRequired());
        }
    }

    /// <summary>
    /// 测试用实体配置查找器，将实体映射到测试 DbContext
    /// </summary>
    private sealed class TestEntityConfigurationTypeFinder : IEntityConfigurationTypeFinder
    {
        private static readonly Type ContextType = typeof(DeleteTestContext);

        public IEnumerable<IEntityTypeConfiguration> GetEntityTypeConfigurations(Type dbContextType) => [];

        public Type GetDbContextTypeForEntity(Type entityType) =>
            ContextType;

        public IEnumerable<Type> GetAllDbContextTypes() => [ContextType];

        public bool HasDbContextForEntity<T>() => true;
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
    /// 构建测试服务容器：注册 DbContext（SQLite 内存库）、设置、实体查找器、
    /// 作用域提供程序、DbContextFactory 与开放泛型仓储
    /// </summary>
    private static TestHost CreateHost()
    {
        var connection = new SqliteConnection("DataSource=:memory:");
        connection.Open();

        var services = new ServiceCollection();
        services.AddSingleton(new DbContextSettings { UseUnderScoreCase = true, DatabaseType = "Sqlite" });
        services.AddSingleton<IEntityConfigurationTypeFinder>(new TestEntityConfigurationTypeFinder());
        services.AddDbContext<DeleteTestContext>(options => options.UseSqlite(connection));
        services.TryAddScoped<DbContextFactory>();
        services.TryAddScoped(typeof(IRepository<,>), typeof(EfRepository<,>));

        var rootProvider = services.BuildServiceProvider();
        services.AddSingleton<IScopeServiceProvider>(new TestScopeServiceProvider(rootProvider));
        var provider = services.BuildServiceProvider();
        provider.GetRequiredService<DeleteTestContext>().Database.EnsureCreated();

        return new TestHost(provider, connection);
    }

    /// <summary>
    /// 异步按主键删除：最小点查，ChangeTracker 仅含父实体（导航集合不被加载），物理删除落库
    /// </summary>
    [Fact]
    public async Task DeleteAsync_ById_DoesNotLoadNavigations()
    {
        using var host = CreateHost();
        using var seedContext = host.Provider.GetRequiredService<DeleteTestContext>();
        seedContext.Orders.Add(new Order("O1")
        {
            Name = "order",
            Children = [new Child(1) { Name = "c1" }, new Child(2) { Name = "c2" }]
        });
        await seedContext.SaveChangesAsync();

        // 新作用域上下文：空 ChangeTracker，模拟独立请求（仓储与上下文须取自同一作用域）
        using var scope = host.Provider.CreateScope();
        using var context = scope.ServiceProvider.GetRequiredService<DeleteTestContext>();
        var repository = new EfRepository<Order, string>(new DbContextFactory(scope.ServiceProvider));

        // Act：按主键删除
        await repository.DeleteAsync("O1");

        // Assert：删除后 ChangeTracker 只含父实体 1 个条目（子集合未被点查加载）
        Assert.Single(context.ChangeTracker.Entries());
        Assert.Contains(context.ChangeTracker.Entries(), x => x.Entity is Order);

        await context.SaveChangesAsync();

        // 物理删除落库；子行由数据库外键级联（required 关系默认 Cascade）删除
        Assert.False(await context.Orders.IgnoreQueryFilters().AnyAsync(x => x.Id == "O1"));
        Assert.Equal(0, await context.Children.CountAsync());
    }

    /// <summary>
    /// 同步按主键删除：与异步版一致，最小点查不加载导航
    /// </summary>
    [Fact]
    public void Delete_ById_DoesNotLoadNavigations()
    {
        using var host = CreateHost();
        using var seedContext = host.Provider.GetRequiredService<DeleteTestContext>();
        seedContext.Orders.Add(new Order("O1")
        {
            Name = "order",
            Children = [new Child(1) { Name = "c1" }, new Child(2) { Name = "c2" }]
        });
        seedContext.SaveChanges();

        using var scope = host.Provider.CreateScope();
        using var context = scope.ServiceProvider.GetRequiredService<DeleteTestContext>();
        var repository = new EfRepository<Order, string>(new DbContextFactory(scope.ServiceProvider));

        // Act
        repository.Delete("O1");

        // Assert：ChangeTracker 仅父实体（旧实现经 Store 会加载全部一级导航）
        Assert.Single(context.ChangeTracker.Entries());
        Assert.Contains(context.ChangeTracker.Entries(), x => x.Entity is Order);

        context.SaveChanges();
        Assert.False(context.Orders.IgnoreQueryFilters().Any(x => x.Id == "O1"));
        Assert.Equal(0, context.Children.Count());
    }

    /// <summary>
    /// 按主键删除软删除实体：第一次删除转为软删除（行保留），再次删除时全局过滤器
    /// 已隐藏该行、视为不存在而跳过，不重复软删。
    /// </summary>
    [Fact]
    public async Task DeleteAsync_SoftDeletedEntity_SoftDeletesOnceAndSkipsOnRepeat()
    {
        using var host = CreateHost();
        using var context = host.Provider.GetRequiredService<DeleteTestContext>();
        var repository = new EfRepository<SoftOrder, string>(new DbContextFactory(host.Provider));

        // Arrange：新增（插入 IsDeleted=false，插入不自动软删除）
        var order = new SoftOrder("S1") { Name = "soft" };
        repository.Add(order);
        await context.SaveChangesAsync();
        Assert.False(order.IsDeleted);

        // Act：第一次按主键删除 → 软删除
        await repository.DeleteAsync("S1");
        await context.SaveChangesAsync();

        // Assert：转为软删除而非物理删除，行仍存在
        Assert.True(order.IsDeleted);
        Assert.Equal(1, await context.SoftOrders.IgnoreQueryFilters().CountAsync(x => x.Id == "S1"));

        // Act：再次按主键删除 → 已软删除，跳过
        await repository.DeleteAsync("S1");
        await context.SaveChangesAsync();

        // Assert：仍只有一条，未产生物理删除
        Assert.Equal(1, await context.SoftOrders.IgnoreQueryFilters().CountAsync(x => x.Id == "S1"));
    }
}
