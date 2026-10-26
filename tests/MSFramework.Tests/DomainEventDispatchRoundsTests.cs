using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using MicroserviceFramework;
using MicroserviceFramework.Domain;
using MicroserviceFramework.Ef;
using MicroserviceFramework.Extensions.DependencyInjection;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Xunit;

namespace MSFramework.Tests;

/// <summary>
/// 领域事件派发轮次回归测试：
/// 达到配置的最大派发轮数后中止保存，且中止时事件保留在聚合上（可诊断/人工处置），
/// 派发轮数上限可经 <see cref="DbContextSettings.MaxDomainEventDispatchRounds"/> 配置
/// </summary>
public class DomainEventDispatchRoundsTests
{
    /// <summary>
    /// 链式领域事件：Handler 处理时会再向聚合追加新事件，形成派发循环
    /// </summary>
    public record ChainedDomainEvent : DomainEvent
    {
        /// <summary>
        /// 累计派发次数（跨轮次）
        /// </summary>
        public static int Count;
    }

    /// <summary>
    /// 链式事件处理器：每次处理都向共享聚合追加新事件
    /// </summary>
    public class ChainedDomainEventHandler : IDomainEventHandler<ChainedDomainEvent>
    {
        public Task HandleAsync(ChainedDomainEvent @event, CancellationToken cancellationToken = default)
        {
            Interlocked.Increment(ref ChainedDomainEvent.Count);
            ChainEntity.Shared.AddDomainEvent(new ChainedDomainEvent());
            return Task.CompletedTask;
        }
    }

    /// <summary>
    /// 承载链式事件的聚合根（共享静态实例，事件由 Handler 追加）
    /// </summary>
    public class ChainEntity : CreationAggregateRoot<string>
    {
        /// <summary>
        /// 共享聚合实例：Handler 无法访问测试方法局部状态，经静态引用追加事件
        /// </summary>
        public static readonly ChainEntity Shared = new("chain-1");

        public ChainEntity(string id) : base(id)
        {
        }
    }

    public class ChainEventContext(DbContextOptions<ChainEventContext> options) : DbContextBase(options)
    {
        public DbSet<ChainEntity> ChainEntities => Set<ChainEntity>();

        protected override void ApplyConfiguration(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<ChainEntity>(builder =>
                builder.Property(x => x.Id).HasMaxLength(36).IsRequired());
        }
    }

    /// <summary>
    /// 测试用实体配置查找器，将 ChainEntity 映射到测试 DbContext
    /// </summary>
    private sealed class TestEntityConfigurationTypeFinder : IEntityConfigurationTypeFinder
    {
        private static readonly System.Collections.Generic.Dictionary<Type, Type> EntityToDbContext = new()
        {
            [typeof(ChainEntity)] = typeof(ChainEventContext)
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
    /// 构建测试宿主：完整框架引导（注册 IMediator 与 IDomainEventHandler）、
    /// SQLite 内存 DbContext 与轮次上限配置
    /// </summary>
    /// <param name="maxRounds">领域事件最大派发轮数</param>
    private static ServiceProvider CreateHost(int maxRounds)
    {
        var connection = new SqliteConnection("DataSource=:memory:");
        connection.Open();

        var services = new ServiceCollection();
        services.AddLogging();
        services.AddMicroserviceFramework(x => x.UseDependencyInjectionLoader());
        services.AddSingleton(new DbContextSettings
        {
            UseUnderScoreCase = true,
            DatabaseType = "Sqlite",
            MaxDomainEventDispatchRounds = maxRounds
        });
        services.AddSingleton<IEntityConfigurationTypeFinder>(new TestEntityConfigurationTypeFinder());
        services.AddDbContext<ChainEventContext>(options => options.UseSqlite(connection));

        var rootProvider = services.BuildServiceProvider();
        services.AddSingleton<IScopeServiceProvider>(new TestScopeServiceProvider(rootProvider));
        services.AddEntityFrameworkExtension();

        var provider = services.BuildServiceProvider();
        provider.UseMicroserviceFramework();
        provider.GetRequiredService<ChainEventContext>().Database.EnsureCreated();
        return provider;
    }

    /// <summary>同步和异步保存均在轮次超限时抛出原始异常，并保留尚未派发的事件。</summary>
    /// <param name="useAsync">为 true 时异步保存，否则验证同步桥接路径。</param>
    [Theory]
    [InlineData(true)]
    [InlineData(false)]
    public async Task SaveChanges_AbortsWhenExceedingConfiguredRounds_AndPreservesEventsAsync(bool useAsync)
    {
        ChainedDomainEvent.Count = 0;
        ChainEntity.Shared.ClearDomainEvents();
        // 初始事件由测试侧加入聚合；Handler 每轮再追加一个新事件
        ChainEntity.Shared.AddDomainEvent(new ChainedDomainEvent());

        try
        {
            var provider = CreateHost(maxRounds: 2);
            using var _ = provider;

            using var scope = provider.CreateScope();
            var context = scope.ServiceProvider.GetRequiredService<ChainEventContext>();
            // 共享聚合实例附着到 ChangeTracker，供 SaveChanges 收集领域事件
            context.Attach(ChainEntity.Shared);

            var exception = useAsync
                ? await Assert.ThrowsAsync<MicroserviceFrameworkException>(() => context.SaveChangesAsync())
                : Assert.Throws<MicroserviceFrameworkException>(() => context.SaveChanges());

            // 轮次 1、2 各派发一次，第 3 轮超过配置上限（2）后中止
            Assert.Equal(2, ChainedDomainEvent.Count);
            // 中止时事件保留在聚合上：业务已回滚，事件痕迹可诊断、可人工补发
            Assert.NotEmpty(ChainEntity.Shared.GetDomainEvents());
            Assert.Contains("已派发 2 轮", exception.Message);
        }
        finally
        {
            // 静态共享实例清理，避免污染其他测试
            ChainEntity.Shared.ClearDomainEvents();
        }
    }
}
