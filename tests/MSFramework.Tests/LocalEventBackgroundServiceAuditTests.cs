using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using MicroserviceFramework;
using MicroserviceFramework.Application;
using MicroserviceFramework.Auditing;
using MicroserviceFramework.Auditing.Model;
using MicroserviceFramework.Domain;
using MicroserviceFramework.Ef;
using MicroserviceFramework.Extensions.DependencyInjection;
using MicroserviceFramework.LocalEvent;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Xunit;

namespace MSFramework.Tests;

/// <summary>
/// <see cref="LocalEventBackgroundService"/> 审计链路回归测试：
/// 验证后台事件处理循环将审计操作写入 <see cref="AuditOperationContext"/>（AsyncLocal）后，
/// 启用审计的事件处理器保存变更时审计实体能被正常收集（Stage 16 AsyncLocal 迁移的回归点），
/// 且每个事件处理完成（含异常路径）后执行流中的审计操作被清理，不泄漏到后续事件。
/// </summary>
public class LocalEventBackgroundServiceAuditTests
{
    /// <summary>
    /// 测试用审计实体（string 主键聚合根）
    /// </summary>
    public class LocalEventAuditOrder : DeletionAggregateRoot<string>
    {
        protected LocalEventAuditOrder() : base(default!)
        {
        }

        public LocalEventAuditOrder(string id, string name) : base(id)
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
    public class LocalEventAuditContext(DbContextOptions<LocalEventAuditContext> options) : DbContextBase(options)
    {
        public DbSet<LocalEventAuditOrder> Orders => Set<LocalEventAuditOrder>();

        protected override void ApplyConfiguration(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<LocalEventAuditOrder>(builder =>
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
            [typeof(LocalEventAuditOrder)] = typeof(LocalEventAuditContext)
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
    /// 测试用会话，提供审计操作创建所需的用户信息与跟踪标识
    /// </summary>
    private sealed class TestSession : ISession
    {
        public string TraceIdentifier { get; private set; } = "trace-1";

        public string UserId { get; private set; } = "user-1";

        public string UserName { get; private set; } = "tester";

        public string Email { get; private set; } = "tester@example.com";

        public string PhoneNumber { get; private set; } = "13800000000";

        public string UserDisplayName { get; private set; } = "测试用户";

        public IReadOnlyCollection<string> Roles { get; private set; } = ["admin"];

        public IReadOnlyCollection<string> Subjects { get; private set; } = ["role:admin"];

        /// <summary>
        /// 快照不包含请求头字段，始终返回 null
        /// </summary>
        /// <param name="field">请求头字段</param>
        /// <returns>恒为 null</returns>
        public string GetValue(SessionField field) => null;

        /// <summary>
        /// 用指定会话覆盖标量字段
        /// </summary>
        /// <param name="session">来源会话，不可为 null</param>
        public void Load(ISession session)
        {
            ArgumentNullException.ThrowIfNull(session);

            TraceIdentifier = session.TraceIdentifier;
            UserId = session.UserId;
            UserName = session.UserName;
            Email = session.Email;
            PhoneNumber = session.PhoneNumber;
            UserDisplayName = session.UserDisplayName;
            Roles = session.Roles?.ToArray() ?? [];
            Subjects = session.Subjects?.ToArray() ?? [];
        }
    }

    /// <summary>
    /// 测试宿主：持有 SQLite 内存连接、服务容器与后台服务，保证资源在测试生命周期内统一释放
    /// </summary>
    private sealed class TestHost : IAsyncDisposable
    {
        public TestHost(ServiceProvider provider, SqliteConnection connection,
            LocalEventBackgroundService backgroundService)
        {
            Provider = provider;
            _connection = connection;
            BackgroundService = backgroundService;
        }

        public ServiceProvider Provider { get; }

        public LocalEventBackgroundService BackgroundService { get; }

        private readonly SqliteConnection _connection;

        public async ValueTask DisposeAsync()
        {
            await BackgroundService.StopAsync(CancellationToken.None);
            _connection.Dispose();
            await Provider.DisposeAsync();
        }
    }

    /// <summary>
    /// 测试事件：实例字段用于在处理器（后台执行流）与测试线程之间传递观察结果与完成信号
    /// </summary>
    public record LocalEventAuditEvent : EventBase
    {
        /// <summary>
        /// 事件序号
        /// </summary>
        public int Order { get; init; }

        /// <summary>
        /// 处理器是否抛异常（验证异常路径清理）
        /// </summary>
        public bool ThrowOnHandle { get; init; }

        /// <summary>
        /// 处理器入口观察到的 <see cref="AuditOperationContext.Value"/>（null 表示已清理或未设置）
        /// </summary>
        public AuditOperation ObservedOperation { get; set; }

        /// <summary>
        /// 处理器执行完成信号（记录观察结果后触发）
        /// </summary>
        public TaskCompletionSource<bool> Processed { get; init; } = new();
    }

    /// <summary>
    /// 仅用于验证已注册描述符但未解析到 handler 时的上下文清理。
    /// </summary>
    public record LocalEventMissingHandlerEvent : EventBase;

    /// <summary>
    /// 会被事件描述符扫描到，但测试宿主会移除其 DI 注册。
    /// </summary>
    public class LocalEventMissingHandler : IEventHandler<LocalEventMissingHandlerEvent>
    {
        public Task HandleAsync(LocalEventMissingHandlerEvent @event, CancellationToken cancellationToken) =>
            Task.CompletedTask;
    }

    /// <summary>
    /// 测试事件处理器：记录处理器实例化时（早于审计操作设置）与入口处执行流中的审计操作，
    /// 可选写入审计实体或抛异常
    /// </summary>
    public class LocalEventAuditEventHandler : IEventHandler<LocalEventAuditEvent>
    {
        private readonly LocalEventAuditContext _context;

        /// <summary>
        /// 处理器实例化时观察到的 <see cref="AuditOperationContext.Value"/>。
        /// 后台服务先经 GetService 实例化处理器、再设置审计操作，因此该值反映
        /// 上一事件处理完成/异常后执行流中的审计操作是否已被清理（应为 null）
        /// </summary>
        public static AuditOperation ObservedValueAtConstruction { get; set; }

        public LocalEventAuditEventHandler(LocalEventAuditContext context)
        {
            _context = context;
            ObservedValueAtConstruction = AuditOperationContext.Value;
        }

        public Task HandleAsync(LocalEventAuditEvent @event, CancellationToken cancellationToken)
        {
            // 记录处理器入口处执行流承载的审计操作，供测试线程断言
            @event.ObservedOperation = AuditOperationContext.Value;
            if (@event.ThrowOnHandle)
            {
                @event.Processed.TrySetResult(true);
                throw new InvalidOperationException("模拟事件处理器异常");
            }

            // 写入实体，由后台服务的 SaveChanges 触发审计实体收集
            _context.Orders.Add(new LocalEventAuditOrder("ORDER-" + @event.Order, "order-" + @event.Order));
            @event.Processed.TrySetResult(true);
            return Task.CompletedTask;
        }
    }

    /// <summary>
    /// 构建测试宿主：注册本地事件发布器（可配置审计开关）、SQLite 内存 DbContext、
    /// EF 扩展服务与测试会话
    /// </summary>
    /// <param name="enableAuditing">是否启用本地事件审计</param>
    /// <returns>测试宿主（后台服务已启动）</returns>
    private static async Task<TestHost> CreateHostAsync(
        bool enableAuditing, bool registerSession = true, bool startService = true)
    {
        var connection = new SqliteConnection("DataSource=:memory:");
        connection.Open();

        var services = new ServiceCollection();
        services.AddLogging();
        services.AddMicroserviceFramework(x =>
        {
            x.UseLocalEventPublisher(options => options.EnableAuditing = enableAuditing);
        });
        // 初始化框架（程序集扫描），AddEntityFrameworkExtension 依赖 Utils.Runtime 类型缓存
        services.AddSingleton(new DbContextSettings { UseUnderScoreCase = true, DatabaseType = "Sqlite" });
        services.AddSingleton<IEntityConfigurationTypeFinder>(new TestEntityConfigurationTypeFinder());
        if (registerSession)
        {
            services.AddScoped<ISession, TestSession>();
        }
        services.AddDbContext<LocalEventAuditContext>(options => options.UseSqlite(connection));
        services.RemoveAll<LocalEventMissingHandler>();

        var rootProvider = services.BuildServiceProvider();
        services.AddSingleton<IScopeServiceProvider>(new TestScopeServiceProvider(rootProvider));

        // 注册 IUnitOfWork→EfUnitOfWork、DbContextFactory 等 EF 扩展服务
        services.AddEntityFrameworkExtension();
        services.AddSingleton<LocalEventBackgroundService>();

        var provider = services.BuildServiceProvider();
        provider.UseMicroserviceFramework();
        provider.GetRequiredService<LocalEventAuditContext>().Database.EnsureCreated();

        var backgroundService = provider.GetRequiredService<LocalEventBackgroundService>();
        if (startService)
        {
            await backgroundService.StartAsync(CancellationToken.None);
        }

        return new TestHost(provider, connection, backgroundService);
    }

    /// <summary>
    /// 反复发布事件直到后台服务实际处理（共享静态管道可能被并发测试宿主消费导致事件丢失，丢失则重试），
    /// 返回被后台服务处理的实例
    /// </summary>
    /// <param name="publisher">事件发布器</param>
    /// <param name="createEvent">事件实例工厂</param>
    /// <param name="timeout">总等待超时</param>
    /// <returns>被后台服务处理的事件实例</returns>
    /// <exception cref="TimeoutException">超时仍未处理时抛出</exception>
    private static async Task<LocalEventAuditEvent> PublishUntilProcessedAsync(
        IEventPublisher publisher, Func<int, LocalEventAuditEvent> createEvent, TimeSpan timeout)
    {
        var deadline = DateTime.UtcNow + timeout;
        var order = 0;
        while (DateTime.UtcNow < deadline)
        {
            var evt = createEvent(++order);
            await publisher.PublishAsync(evt);
            var completed = await Task.WhenAny(evt.Processed.Task, Task.Delay(50));
            if (completed == evt.Processed.Task)
            {
                return evt;
            }
        }

        throw new TimeoutException("等待本地事件后台服务处理超时");
    }

    /// <summary>
    /// 轮询等待条件成立，超时抛出异常
    /// </summary>
    /// <param name="condition">等待的条件</param>
    /// <param name="timeout">总等待超时</param>
    /// <exception cref="TimeoutException">超时仍未成立时抛出</exception>
    private static async Task WaitUntilAsync(Func<bool> condition, TimeSpan timeout)
    {
        var deadline = DateTime.UtcNow + timeout;
        while (!condition())
        {
            if (DateTime.UtcNow >= deadline)
            {
                throw new TimeoutException("等待条件成立超时");
            }

            await Task.Delay(20);
        }
    }

    /// <summary>
    /// 启用审计时：事件处理器执行流中承载审计操作，保存变更后审计实体被收集到该操作中
    /// （Stage 16 AsyncLocal 迁移的回归点——后台服务路径实体收集不得丢失）
    /// </summary>
    [Fact]
    public async Task EnableAuditing_EventHandlerSaveChanges_CollectsAuditEntities()
    {
        await using var host = await CreateHostAsync(enableAuditing: true);
        var publisher = host.Provider.GetRequiredService<IEventPublisher>();

        var evt = await PublishUntilProcessedAsync(publisher,
            order => new LocalEventAuditEvent { Order = order }, TimeSpan.FromSeconds(15));

        // 处理器入口必须能观察到执行流承载的审计操作
        Assert.NotNull(evt.ObservedOperation);
        var operation = evt.ObservedOperation;

        // 保存发生在处理器返回之后，等待审计实体收集完成
        await WaitUntilAsync(() => operation.Entities.Count == 1, TimeSpan.FromSeconds(5));

        var entity = Assert.Single(operation.Entities);
        Assert.Equal("ORDER-" + evt.Order, entity.EntityId);
        Assert.Equal(OperationType.Add, entity.OperationType);
        Assert.Contains(nameof(LocalEventAuditOrder), entity.Type);
        Assert.Same(operation, entity.Operation);
    }

    /// <summary>
    /// 审计开启但宿主未注册 ISession 时，事件处理仍必须执行。
    /// </summary>
    [Fact]
    public async Task EnableAuditing_WithoutSession_EventHandlerStillRuns()
    {
        await using var host = await CreateHostAsync(enableAuditing: true, registerSession: false);
        var publisher = host.Provider.GetRequiredService<IEventPublisher>();

        var evt = await PublishUntilProcessedAsync(publisher,
            order => new LocalEventAuditEvent { Order = order }, TimeSpan.FromSeconds(15));

        Assert.NotNull(evt.ObservedOperation);
        Assert.Null(evt.ObservedOperation.TraceId);
    }

    /// <summary>
    /// 未解析 handler 的事件不得把审计上下文泄漏给后续事件。
    /// </summary>
    [Fact]
    public async Task EnableAuditing_MissingHandler_ContextValueClearedBeforeNextEvent()
    {
        LocalEventAuditEventHandler.ObservedValueAtConstruction = null;
        AuditOperationContext.Value = new AuditOperation(
            "seed", null, null, null, null, null, null, "seed-trace", "Local");
        await using var host = await CreateHostAsync(enableAuditing: true, startService: false);
        var publisher = host.Provider.GetRequiredService<IEventPublisher>();

        var missingRegistration = host.Provider.GetRequiredService<EventDescriptorStore>();
        Assert.Contains(missingRegistration.GetList(typeof(LocalEventMissingHandlerEvent)),
            descriptor => descriptor.HandlerType == typeof(LocalEventMissingHandler));

        var next = new LocalEventAuditEvent { Order = 100 };
        await Task.WhenAll(
            publisher.PublishAsync(new LocalEventMissingHandlerEvent()),
            publisher.PublishAsync(next));
        await host.BackgroundService.StartAsync(CancellationToken.None);
        await next.Processed.Task.WaitAsync(TimeSpan.FromSeconds(15));

        Assert.Null(LocalEventAuditEventHandler.ObservedValueAtConstruction);
        Assert.NotNull(next.ObservedOperation);
        AuditOperationContext.Value = null;
    }

    /// <summary>
    /// 事件处理完成后（SaveChanges 之后）执行流中的审计操作必须被清理：
    /// 后续事件在设置新审计操作之前（处理器实例化时）不得观察到上一事件的残留值
    /// </summary>
    [Fact]
    public async Task EnableAuditing_AfterEventCompleted_ContextValueCleared()
    {
        LocalEventAuditEventHandler.ObservedValueAtConstruction = null;
        await using var host = await CreateHostAsync(enableAuditing: true);
        var publisher = host.Provider.GetRequiredService<IEventPublisher>();

        var first = await PublishUntilProcessedAsync(publisher,
            order => new LocalEventAuditEvent { Order = order }, TimeSpan.FromSeconds(15));
        Assert.NotNull(first.ObservedOperation);
        await WaitUntilAsync(() => first.ObservedOperation.Entities.Count == 1, TimeSpan.FromSeconds(5));

        // 第二个事件：处理器实例化发生在审计操作设置之前，此时必须观察不到上一事件的残留值
        var second = await PublishUntilProcessedAsync(publisher,
            order => new LocalEventAuditEvent { Order = 100 + order }, TimeSpan.FromSeconds(15));
        Assert.Null(LocalEventAuditEventHandler.ObservedValueAtConstruction);
        // 处理器入口观察到的必须是第二个事件自身的审计操作，而非上一事件的残留值
        Assert.NotNull(second.ObservedOperation);
        Assert.NotSame(first.ObservedOperation, second.ObservedOperation);
    }

    /// <summary>
    /// 事件处理器抛异常时（异常路径），执行流中的审计操作同样必须被清理（try/finally 兜底）：
    /// 后续事件在设置新审计操作之前（处理器实例化时）不得观察到残留值
    /// </summary>
    [Fact]
    public async Task EnableAuditing_EventHandlerThrows_ContextValueCleared()
    {
        LocalEventAuditEventHandler.ObservedValueAtConstruction = null;
        await using var host = await CreateHostAsync(enableAuditing: true);
        var publisher = host.Provider.GetRequiredService<IEventPublisher>();

        var throwing = await PublishUntilProcessedAsync(publisher,
            order => new LocalEventAuditEvent { Order = order, ThrowOnHandle = true },
            TimeSpan.FromSeconds(15));
        // 抛异常前处理器入口已观察到审计操作（证明值被设置过）
        Assert.NotNull(throwing.ObservedOperation);

        // 异常处理后的事件：处理器实例化时必须观察不到残留值（finally 已清理）
        var next = await PublishUntilProcessedAsync(publisher,
            order => new LocalEventAuditEvent { Order = 100 + order }, TimeSpan.FromSeconds(15));
        Assert.Null(LocalEventAuditEventHandler.ObservedValueAtConstruction);
        Assert.NotNull(next.ObservedOperation);
        Assert.NotSame(throwing.ObservedOperation, next.ObservedOperation);
    }

    /// <summary>
    /// 未启用审计时：事件处理器执行流中不得承载任何审计操作
    /// </summary>
    [Fact]
    public async Task DisableAuditing_EventHandler_NoAuditOperationInContext()
    {
        await using var host = await CreateHostAsync(enableAuditing: false);
        var publisher = host.Provider.GetRequiredService<IEventPublisher>();

        var evt = await PublishUntilProcessedAsync(publisher,
            order => new LocalEventAuditEvent { Order = order }, TimeSpan.FromSeconds(15));

        Assert.Null(evt.ObservedOperation);
    }
}
