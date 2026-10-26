using System;
using System.Linq;
using System.Threading.Tasks;
using MicroserviceFramework;
using MicroserviceFramework.Domain;
using MicroserviceFramework.Ef;
using MicroserviceFramework.Extensions.DependencyInjection;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.Extensions.DependencyInjection;
using Xunit;

namespace MSFramework.Tests;

/// <summary>
/// 主上下文单一事实来源回归测试：
/// <see cref="EfUnitOfWork"/> 以 <see cref="DbContextSettings.IsPrimary"/> 判定主（可写）上下文
/// （未显式配置时回退为第一个），提交目标与迁移目标共用同一判定，
/// 避免多上下文应用中"迁移落到 A 库、业务提交写到 B 库"的 schema 错位
/// </summary>
public class EfUnitOfWorkPrimaryContextTests
{
    /// <summary>
    /// 上下文 A 的实体（非主上下文，按约定只读）
    /// </summary>
    public class EntityA : CreationAggregateRoot<string>
    {
        public EntityA(string id) : base(id)
        {
        }
    }

    /// <summary>
    /// 上下文 B 的实体（主上下文，参与提交）
    /// </summary>
    public class EntityB : CreationAggregateRoot<string>
    {
        public EntityB(string id) : base(id)
        {
        }
    }

    public class PrimaryContextA(DbContextOptions<PrimaryContextA> options) : DbContextBase(options)
    {
        public DbSet<EntityA> Entities => Set<EntityA>();

        protected override void ApplyConfiguration(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<EntityA>(builder => builder.Property(x => x.Id).HasMaxLength(36).IsRequired());
        }
    }

    public class PrimaryContextB(DbContextOptions<PrimaryContextB> options) : DbContextBase(options)
    {
        public DbSet<EntityB> Entities => Set<EntityB>();

        protected override void ApplyConfiguration(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<EntityB>(builder => builder.Property(x => x.Id).HasMaxLength(36).IsRequired());
        }
    }

    /// <summary>
    /// 测试用实体配置查找器：EntityA → 上下文 A，EntityB → 上下文 B
    /// </summary>
    private sealed class TestEntityConfigurationTypeFinder : IEntityConfigurationTypeFinder
    {
        private static readonly System.Collections.Generic.Dictionary<Type, Type> EntityToDbContext = new()
        {
            [typeof(EntityA)] = typeof(PrimaryContextA),
            [typeof(EntityB)] = typeof(PrimaryContextB)
        };

        public System.Collections.Generic.IEnumerable<IEntityTypeConfiguration> GetEntityTypeConfigurations(
            Type dbContextType) => [];

        public Type GetDbContextTypeForEntity(Type entityType) =>
            EntityToDbContext.TryGetValue(entityType, out var contextType) ? contextType : null;

        public System.Collections.Generic.IEnumerable<Type> GetAllDbContextTypes() =>
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
    /// 构建双上下文测试宿主，各上下文挂载独立的 DbContextSettings（经 options 扩展注入）
    /// </summary>
    /// <param name="settingsA">上下文 A 的设置</param>
    /// <param name="settingsB">上下文 B 的设置</param>
    /// <returns>(服务提供器, 释放委托)</returns>
    private static (ServiceProvider Provider, Action Dispose) CreateHost(
        DbContextSettings settingsA, DbContextSettings settingsB)
    {
        var connectionA = new SqliteConnection("DataSource=:memory:");
        connectionA.Open();
        var connectionB = new SqliteConnection("DataSource=:memory:");
        connectionB.Open();

        var services = new ServiceCollection();
        services.AddSingleton<IEntityConfigurationTypeFinder>(new TestEntityConfigurationTypeFinder());
        services.AddDbContext<PrimaryContextA>(options =>
        {
            options.UseSqlite(connectionA);
            ((IDbContextOptionsBuilderInfrastructure)options).AddOrUpdateExtension(settingsA);
        });
        services.AddDbContext<PrimaryContextB>(options =>
        {
            options.UseSqlite(connectionB);
            ((IDbContextOptionsBuilderInfrastructure)options).AddOrUpdateExtension(settingsB);
        });
        services.AddEntityFrameworkExtension();

        // EF 的 this.GetService<T>() 对未注册服务会抛出而非返回 null：
        // 按既有回归测试模式，先建根容器再挂 TestScopeServiceProvider 后重建
        var rootProvider = services.BuildServiceProvider();
        services.AddSingleton<IScopeServiceProvider>(new TestScopeServiceProvider(rootProvider));
        var provider = services.BuildServiceProvider();

        provider.GetRequiredService<PrimaryContextA>().Database.EnsureCreated();
        provider.GetRequiredService<PrimaryContextB>().Database.EnsureCreated();

        return (provider, () =>
        {
            connectionA.Dispose();
            connectionB.Dispose();
            provider.Dispose();
        });
    }

    private static DbContextSettings CreateSettings(bool isPrimary) =>
        new() { UseUnderScoreCase = true, DatabaseType = "Sqlite", IsPrimary = isPrimary };

    [Fact]
    public async Task SaveChangesAsync_IsPrimaryFlagDecidesCommitTargetAsync()
    {
        var (provider, dispose) = CreateHost(CreateSettings(false), CreateSettings(true));
        try
        {
            using var scope = provider.CreateScope();
            var unitOfWork = scope.ServiceProvider.GetRequiredService<IUnitOfWork>();

            // 主上下文为 B（IsPrimary=true）：B 的新增实体应被 UoW 提交
            var contextB = scope.ServiceProvider.GetRequiredService<PrimaryContextB>();
            var addedEntityB = new EntityB("b-1");
            contextB.Set<EntityB>().Add(addedEntityB);
            await unitOfWork.SaveChangesAsync();

            // 上下文 A 的变更不参与提交（约定：非主上下文只读，变更随作用域丢弃）
            var contextA = scope.ServiceProvider.GetRequiredService<PrimaryContextA>();
            var addedEntityA = new EntityA("a-1");
            contextA.Set<EntityA>().Add(addedEntityA);
            await unitOfWork.SaveChangesAsync();

            Assert.Equal(EntityState.Unchanged, contextB.Entry(addedEntityB).State);
            Assert.Equal(EntityState.Added, contextA.Entry(addedEntityA).State);
        }
        finally
        {
            dispose();
        }
    }

    [Fact]
    public async Task SaveChangesAsync_MultiplePrimaryContexts_ThrowsInsteadOfAmbiguousCommitAsync()
    {
        var (provider, dispose) = CreateHost(CreateSettings(true), CreateSettings(true));
        try
        {
            using var scope = provider.CreateScope();
            var unitOfWork = scope.ServiceProvider.GetRequiredService<IUnitOfWork>();

            // 两个上下文同时标记为主上下文属配置矛盾：显式抛出，不得静默选其一
            var exception = await Assert.ThrowsAsync<MicroserviceFrameworkException>(
                () => unitOfWork.SaveChangesAsync());
            Assert.Contains("主（可写）上下文", exception.Message);
        }
        finally
        {
            dispose();
        }
    }
}
