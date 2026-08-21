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
public class LocalEventBackgroundService(
    IServiceProvider serviceProvider,
    ILogger<LocalEventBackgroundService> logger,
    EventDescriptorStore descriptorStore,
    IOptions<LocalEventOptions> options)
    : BackgroundService
{
    private readonly LocalEventChannel _eventChannel =
        serviceProvider.GetRequiredService<LocalEventChannel>();

    /// <summary>
    /// 后台执行循环，持续消费事件管道中的事件并分发处理。
    /// </summary>
    /// <param name="stoppingToken">取消令牌</param>
    /// <returns>表示异步操作的任务</returns>
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        logger.LogInformation("本地事件服务启动");

        while (await _eventChannel.EventChannel.Reader.WaitToReadAsync(stoppingToken))
        {
            while (_eventChannel.EventChannel.Reader.TryRead(out var entry))
            {
                try
                {
                    var traceId = entry.Session == null
                        ? ObjectId.GenerateNewId().ToString()
                        : entry.Session.TraceIdentifier;
                    var eventType = entry.EventData.GetType();
                    var descriptors = descriptorStore.GetList(eventType);

                    foreach (var descriptor in descriptors)
                    {
                        var handlerName = descriptor.HandlerType.FullName;

                        logger.LogDebug("{TraceId}, 事件处理器 {HandlerType} 执行开始", traceId, handlerName);

                        using var scope = serviceProvider.CreateScope();
                        var services = scope.ServiceProvider;
                        using var scopeContext = ScopeServiceProviderContext.Push(
                            new LocalScopeServiceProvider(services));
                        try
                        {
                            // 每个 handler 都从干净的执行流开始，避免上一个事件的审计操作泄漏。
                            AuditOperationContext.Value = null;

                            // 先恢复发布时的会话快照，再解析 handler，保证 handler 构造函数及其 DbContext
                            // 看到的是发布者身份而不是后台根容器的默认会话。
                            var session = services.GetService<ISession>();
                            if (session != null && entry.Session != null)
                            {
                                session.Load(entry.Session);
                            }

                            var handler = services.GetService(descriptor.HandlerType);
                            if (handler == null)
                            {
                                continue;
                            }

                            AuditOperation auditOperation = null;
                            if (options.Value.EnableAuditing)
                            {
                                auditOperation = CreateAuditedOperation(session, handlerName);
                                // 审计操作承载到当前后台执行流（AsyncLocal），使 DbContextBase 默认保存流程
                                // 能在同一执行流读取到本事件处理器的审计操作并收集变更实体；
                                // 每个事件处理器是独立审计单元，处理完成后（含异常路径）必须在 finally 清理
                                AuditOperationContext.Value = auditOperation;
                            }

                            if (descriptor.HandleMethod.Invoke(handler, [entry.EventData, stoppingToken]) is
                                not Task
                                task)
                            {
                                continue;
                            }

                            await task;
                            var unitOfWork = services.GetService<IUnitOfWork>();
                            if (unitOfWork != null)
                            {
                                await unitOfWork.SaveChangesAsync(stoppingToken);
                            }

                            if (auditOperation != null)
                            {
                                IReadOnlyCollection<IAuditingStore> auditingStores = [];
                                try
                                {
                                    // 在业务处理完成后才解析审计存储，存储构造或落库失败不会丢弃业务事件。
                                    auditingStores = services.GetServices<IAuditingStore>().ToArray();
                                }
                                catch (Exception e)
                                {
                                    logger.LogError(e, "{TraceId}, 解析审计存储失败", traceId);
                                }

                                await SaveAuditOperation(auditOperation, auditingStores, traceId, handlerName);
                            }

                            logger.LogDebug(
                                "{TraceId}, 事件处理器 {HandlerType} 执行结束", traceId, handlerName);
                        }
                        catch (Exception e)
                        {
                            logger.LogError(e, "{TraceId}, 事件处理器 {HandlerType} 执行失败",
                                traceId, handlerName);
                        }
                        finally
                        {
                            // 包括 handler 未解析、处理器异常和正常完成在内，统一清理执行流中的审计操作，
                            // 防止 AsyncLocal 值污染后续事件处理器或其他请求
                            AuditOperationContext.Value = null;
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

    /// <summary>
    /// 停止后台事件服务，发送关闭信号。
    /// </summary>
    /// <param name="cancellationToken">取消令牌</param>
    /// <returns>表示异步操作的任务</returns>
    public override async Task StopAsync(CancellationToken cancellationToken)
    {
        logger.LogDebug("开始关闭本地事件服务");
        _eventChannel.EventChannel.Writer.TryComplete();
        await base.StopAsync(cancellationToken);
        logger.LogInformation("关闭本地事件服务完成");
    }

    private AuditOperation CreateAuditedOperation(ISession session, string handlerType)
    {
        // session 可能为 null（ISession 未注册的宿主），审计操作仅缺省用户信息，不影响事件处理
        var auditedOperation = new AuditOperation(handlerType, null, null, null, null,
            null, null, session?.TraceIdentifier, "Local");
        auditedOperation.SetCreation(session?.UserId, session?.UserDisplayName, DateTimeOffset.UtcNow);
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
