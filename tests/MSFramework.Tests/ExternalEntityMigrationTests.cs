using System;
using System.Collections.Generic;
using System.Linq;
using MicroserviceFramework.Domain;
using MicroserviceFramework.Ef;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.Extensions.DependencyInjection;
using Xunit;

namespace MSFramework.Tests;

/// <summary>
/// 外部实体迁移排除回归测试：
/// 验证实现 <see cref="IExternalEntity"/> 的实体一律被排除出迁移
/// （<c>SetIsTableExcludedFromMigrations(true)</c>）——
/// 模型构建管道（<c>EntityTableConventionStrategy</c>）按类型统一判定，
/// 即使业务没有编写对应的实体配置类（<see cref="EntityTypeConfigurationBase{TEntity,TDbContext}"/>
/// 只覆盖"写了配置"的场景），迁移也不会生成其建表/改表/删表脚本。
/// </summary>
public class ExternalEntityMigrationTests
{
    /// <summary>
    /// 无审计接口的外部实体：仅继承 <see cref="ExternalEntity{TKey}"/>，且故意不提供实体配置类
    /// </summary>
    public class RemoteOrder : ExternalEntity<int>
    {
        public RemoteOrder(int id) : base(id)
        {
        }

        /// <summary>
        /// 外部订单号
        /// </summary>
        public string OrderNo { get; set; }
    }

    /// <summary>
    /// 受管实体：作为对照组，必须参与迁移
    /// </summary>
    public class LocalProduct : EntityBase<int>
    {
        public LocalProduct(int id) : base(id)
        {
        }

        /// <summary>
        /// 名称
        /// </summary>
        public string Name { get; set; }
    }

    /// <summary>
    /// 测试用 DbContext：两个实体均不编写实体配置类（ApplyConfiguration 为空）
    /// </summary>
    public class ExternalEntityContext(DbContextOptions<ExternalEntityContext> options) : DbContextBase(options)
    {
        public DbSet<RemoteOrder> RemoteOrders => Set<RemoteOrder>();

        public DbSet<LocalProduct> LocalProducts => Set<LocalProduct>();

        protected override void ApplyConfiguration(ModelBuilder modelBuilder)
        {
        }
    }

    /// <summary>
    /// 测试用实体配置查找器，将实体映射到测试 DbContext
    /// </summary>
    private sealed class TestEntityConfigurationTypeFinder : IEntityConfigurationTypeFinder
    {
        private static readonly Dictionary<Type, Type> EntityToDbContext = new()
        {
            [typeof(RemoteOrder)] = typeof(ExternalEntityContext),
            [typeof(LocalProduct)] = typeof(ExternalEntityContext)
        };

        public IEnumerable<IEntityTypeConfiguration> GetEntityTypeConfigurations(Type dbContextType) => [];

        public Type GetDbContextTypeForEntity(Type entityType) =>
            EntityToDbContext.TryGetValue(entityType, out var contextType) ? contextType : null;

        public IEnumerable<Type> GetAllDbContextTypes() =>
            EntityToDbContext.Values.Distinct();

        public bool HasDbContextForEntity<T>() => EntityToDbContext.ContainsKey(typeof(T));
    }

    /// <summary>
    /// 构建 DI 宿主并解析测试 DbContext（OnModelCreating 需要从 DI 解析 DbContextSettings 与配置查找器）；
    /// "是否从迁移排除"标记只存于设计时模型，断言须读 <see cref="IDesignTimeModel"/>
    /// </summary>
    private static (ExternalEntityContext Context, ServiceProvider Provider) CreateContext()
    {
        var services = new ServiceCollection();
        services.AddSingleton(new DbContextSettings { UseUnderScoreCase = true, DatabaseType = "Sqlite" });
        services.AddSingleton<IEntityConfigurationTypeFinder>(new TestEntityConfigurationTypeFinder());
        services.AddDbContext<ExternalEntityContext>(options => options.UseSqlite("DataSource=:memory:"));

        var provider = services.BuildServiceProvider();
        return (provider.GetRequiredService<ExternalEntityContext>(), provider);
    }

    /// <summary>
    /// 外部实体必须从迁移中排除（即使没有编写实体配置类）
    /// </summary>
    [Fact]
    public void Model_ExternalEntityWithoutConfiguration_IsExcludedFromMigrations()
    {
        var (context, provider) = CreateContext();
        using (provider)
        using (context)
        {
            var entityType = context.GetService<IDesignTimeModel>().Model.FindEntityType(typeof(RemoteOrder));

            Assert.NotNull(entityType);
            Assert.True(entityType.IsTableExcludedFromMigrations());
        }
    }

    /// <summary>
    /// 对照：受管实体不受外部实体排除逻辑影响，正常参与迁移
    /// </summary>
    [Fact]
    public void Model_ManagedEntityWithoutConfiguration_IsNotExcludedFromMigrations()
    {
        var (context, provider) = CreateContext();
        using (provider)
        using (context)
        {
            var entityType = context.GetService<IDesignTimeModel>().Model.FindEntityType(typeof(LocalProduct));

            Assert.NotNull(entityType);
            Assert.False(entityType.IsTableExcludedFromMigrations());
        }
    }
}
