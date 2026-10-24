using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using MicroserviceFramework.Application;
using MicroserviceFramework.Auditing;
using MicroserviceFramework.Auditing.Model;
using MicroserviceFramework.Domain;
using MicroserviceFramework.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using MongoDB.Bson;

namespace MicroserviceFramework.LocalEvent;

/// <summary>
/// 本地事件后台服务，从管道中消费事件并分发到注册的处理器。
/// </summary>
/// <param name="serviceProvider">服务提供程序</param>
/// <param name="logger">日志记录器</param>
/// <param name="descriptorStore">事件处理器描述符存储</param>
/// <param name="options">本地事件配置</param>
/// <param name="deadLetterStore">死信存储</param>
public class LocalEventBackgroundService(
    IServiceProvider serviceProvider,
    ILogger<LocalEventBackgroundService> logger,
    EventDescriptorStore descriptorStore,
    IOptions<LocalEventOptions> options,
    ILocalEventDeadLetterStore deadLetterStore)
    : BackgroundService
{
    private readonly LocalEventChannel _eventChannel =
        serviceProvider.GetRequiredService<LocalEventChannel>();
    private readonly TaskCompletionSource _drained =
        new(TaskCreationOptions.RunContinuationsAsynchronously);
    private int _started;

    /// <inheritdoc />
    public override Task StartAsync(CancellationToken cancellationToken)
    {
        Interlocked.Exchange(ref _started, 1);
        return base.StartAsync(cancellationToken);
    }

    /// <summary>
    /// 后台执行循环，持续消费事件管道中的事件并分发处理。
    /// </summary>
    /// <param name="stoppingToken">取消令牌</param>
    /// <returns>表示异步操作的任务</returns>
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        logger.LogInformation("本地事件服务启动");
        try
        {
            while (await _eventChannel.EventChannel.Reader.WaitToReadAsync(stoppingToken))
            {
                while (_eventChannel.EventChannel.Reader.TryRead(out var entry))
                {
                    try
                    {
                        var traceId = entry.Session == null
                            ? ObjectId.GenerateNewId().ToString()
                            : entry.Session.TraceIdentifier;
                        var descriptors = descriptorStore.GetList(entry.EventData.GetType());

                        foreach (var descriptor in descriptors)
                        {
                            Exception lastError = null;
                            var maxAttempts = Math.Max(1, options.Value.MaxDeliveryAttempts);
                            for (var attempt = 1; attempt <= maxAttempts; attempt++)
                            {
                                try
                                {
                                    await ProcessHandlerAsync(entry.Session, entry.EventData, descriptor,
                                        traceId, stoppingToken);
                                    lastError = null;
                                    break;
                                }
                                catch (Exception e)
                                {
                                    lastError = e;
                                    logger.LogWarning(e,
                                        "{TraceId}, 事件处理器 {HandlerType} 第 {Attempt}/{MaxAttempts} 次执行失败",
                                        traceId, descriptor.HandlerType.FullName, attempt, maxAttempts);
                                }

                                if (attempt < maxAttempts && options.Value.RetryDelay > TimeSpan.Zero)
                                {
                                    await Task.Delay(options.Value.RetryDelay, stoppingToken);
                                }
                            }

                            if (lastError != null)
                            {
                                var deadLetter = new LocalEventDeadLetter(entry.EventData,
                                    descriptor.HandlerType, maxAttempts, lastError, DateTimeOffset.UtcNow);
                                try
                                {
                                    await deadLetterStore.StoreAsync(deadLetter, stoppingToken);
                                }
                                catch (Exception e)
                                {
                                    logger.LogError(e,
                                        "{TraceId}, 事件处理器 {HandlerType} 写入死信失败",
                                        traceId, descriptor.HandlerType.FullName);
                                }
                            }
                        }
                    }
                    catch (Exception e)
                    {
                        logger.LogError(e, "处理事件失败: {EventData}",
                            Defaults.JsonSerializer.Serialize(entry));
                    }
                }
            }
        }
        finally
        {
            _drained.TrySetResult();
        }
    }

    private async Task ProcessHandlerAsync(SessionSnapshot sessionSnapshot, EventBase eventData,
        EventDescriptor descriptor, string traceId, CancellationToken cancellationToken)
    {
        var handlerName = descriptor.HandlerType.FullName;
        logger.LogDebug("{TraceId}, 事件处理器 {HandlerType} 执行开始", traceId, handlerName);

        using var scope = serviceProvider.CreateScope();
        var services = scope.ServiceProvider;
        using var scopeContext = ScopeServiceProviderContext.Push(new LocalScopeServiceProvider(services));
        try
        {
            AuditOperationContext.Value = null;
            var session = services.GetService<ISession>();
            if (session != null && sessionSnapshot != null)
            {
                session.Load(sessionSnapshot);
            }

            var handler = services.GetService(descriptor.HandlerType);
            if (handler == null)
            {
                return;
            }

            AuditOperation auditOperation = null;
            if (options.Value.EnableAuditing)
            {
                auditOperation = CreateAuditedOperation(session, handlerName);
                AuditOperationContext.Value = auditOperation;
            }

            if (descriptor.HandleMethod.Invoke(handler, [eventData, cancellationToken]) is not Task task)
            {
                return;
            }

            await task;
            var unitOfWork = services.GetService<IUnitOfWork>();
            if (unitOfWork != null)
            {
                await unitOfWork.SaveChangesAsync(cancellationToken);
            }

            if (auditOperation != null)
            {
                IReadOnlyCollection<IAuditingStore> auditingStores = [];
                try
                {
                    auditingStores = services.GetServices<IAuditingStore>().ToArray();
                }
                catch (Exception e)
                {
                    logger.LogError(e, "{TraceId}, 解析审计存储失败", traceId);
                }

                await SaveAuditOperation(auditOperation, auditingStores, traceId, handlerName);
            }

            logger.LogDebug("{TraceId}, 事件处理器 {HandlerType} 执行结束", traceId, handlerName);
        }
        finally
        {
            AuditOperationContext.Value = null;
        }
    }

    /// <summary>
    /// 停止后台事件服务，发送关闭信号。
    /// </summary>
    /// <param name="cancellationToken">取消令牌</param>
    /// <returns>表示异步操作的任务</returns>
    public override async Task StopAsync(CancellationToken cancellationToken)
    {
        logger.LogDebug("开始关闭本地事件服务");
        _eventChannel.EventChannel.Writer.TryComplete();
        if (Volatile.Read(ref _started) != 0)
        {
            try
            {
                await _drained.Task.WaitAsync(options.Value.ShutdownDrainTimeout, cancellationToken);
            }
            catch (TimeoutException)
            {
                logger.LogWarning("本地事件服务在 {Timeout} 内未完成排空，将取消剩余处理",
                    options.Value.ShutdownDrainTimeout);
            }
        }

        await base.StopAsync(cancellationToken);
        logger.LogInformation("关闭本地事件服务完成");
    }

    private AuditOperation CreateAuditedOperation(ISession session, string handlerType)
    {
        // session 可能为 null（ISession 未注册的宿主），审计操作仅缺省用户信息，不影响事件处理
        var auditedOperation = new AuditOperation(handlerType, null, null, null, null,
            null, null, session?.TraceIdentifier, "Local");
        auditedOperation.SetCreation(session?.UserId, session?.UserDisplayName, DateTimeOffset.Now);
        return auditedOperation;
    }

    /// <summary>
    /// 结束并保存后台事件审计。单个存储失败只记录日志，不影响已完成的业务事件。
    /// </summary>
    private async Task SaveAuditOperation(AuditOperation auditOperation,
        IReadOnlyCollection<IAuditingStore> auditingStores, string traceId, string handlerName)
    {
        auditOperation.End();
        foreach (var auditingStore in auditingStores)
        {
            try
            {
                await auditingStore.AddAsync(auditOperation);
            }
            catch (Exception e)
            {
                logger.LogError(e, "{TraceId}, 事件处理器 {HandlerType} 保存审计信息失败",
                    traceId, handlerName);
            }
        }
    }

    private sealed class LocalScopeServiceProvider(IServiceProvider provider) : IScopeServiceProvider
    {
        public T GetService<T>()
        {
            return provider.GetService<T>();
        }
    }
}
