using System;
using System.Threading;
using System.Threading.Tasks;
using MicroserviceFramework.Application;
using MicroserviceFramework.Auditing;
using MicroserviceFramework.Auditing.Model;
using MicroserviceFramework.Domain;
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
    /// <summary>
    /// 后台执行循环，持续消费事件管道中的事件并分发处理。
    /// </summary>
    /// <param name="stoppingToken">取消令牌</param>
    /// <returns>表示异步操作的任务</returns>
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        logger.LogInformation("本地事件服务启动");

        while (await LocalEventPublisher.EventChannel.Reader.WaitToReadAsync(stoppingToken))
        {
            while (LocalEventPublisher.EventChannel.Reader.TryRead(out var entry))
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
                        var handler = services.GetService(descriptor.HandlerType);
                        if (handler == null)
                        {
                            continue;
                        }

                        var session = services.GetService<ISession>();
                        // 覆盖 session 对象
                        if (entry.Session != null)
                        {
                            session?.Load(entry.Session);
                        }

                        if (options.Value.EnableAuditing)
                        {
                            var auditOperation = CreateAuditedOperation(session, handlerName);
                            var unitOfWork = services.GetService<IUnitOfWork>();
                            unitOfWork?.RegisterAuditOperation(auditOperation);

                            // 审计操作承载到当前后台执行流（AsyncLocal），使保存回调（OnSavingChanges）
                            // 能在同一执行流读取到本事件处理器的审计操作并收集变更实体；
                            // 每个事件处理器是独立审计单元，处理完成后（含异常路径）必须在 finally 清理
                            AuditOperationContext.Value = auditOperation;
                        }

                        try
                        {
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
                            // 事件处理完成或异常后清理执行流中的审计操作，防止 AsyncLocal 值
                            // 随 ExecutionContext 复用到后续事件处理器或其他请求
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
        await base.StopAsync(cancellationToken);
        logger.LogInformation("关闭本地事件服务完成");
    }

    private AuditOperation CreateAuditedOperation(ISession session, string handlerType)
    {
        var auditedOperation = new AuditOperation(handlerType, null, null, null, null,
            null, null, session.TraceIdentifier, "Local");
        auditedOperation.SetCreation(session.UserId, session.UserDisplayName, DateTimeOffset.UtcNow);
        return auditedOperation;
    }
}
