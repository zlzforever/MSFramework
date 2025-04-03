using System;
using System.Threading;
using System.Threading.Tasks;
using MicroserviceFramework.Application;
using MicroserviceFramework.Auditing.Model;
using MicroserviceFramework.Domain;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using MongoDB.Bson;

namespace MicroserviceFramework.LocalEvent;

/// <summary>
///
/// </summary>
/// <param name="serviceProvider"></param>
/// <param name="logger"></param>
/// <param name="descriptorStore"></param>
/// <param name="options"></param>
public class LocalEventService(
    IServiceProvider serviceProvider,
    ILogger<LocalEventService> logger,
    EventDescriptorStore descriptorStore,
    IOptions<LocalEventOptions> options)
    : BackgroundService
{
    /// <summary>
    ///
    /// </summary>
    /// <param name="stoppingToken"></param>
    /// <returns></returns>
    protected override Task ExecuteAsync(CancellationToken stoppingToken)
    {
        return Task.Factory.StartNew(async () =>
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
                                return;
                            }

                            var session = services.GetService<ISession>();
                            // 覆盖 session 对象
                            if (entry.Session != null)
                            {
                                session?.Load(entry.Session);
                            }

                            if (options.Value.EnableAuditing)
                            {
                                var unitOfWork = services.GetService<IUnitOfWork>();
                                unitOfWork?.SetAuditOperationFactory(() =>
                                    CreateAuditedOperation(session, handlerName));
                            }

                            if (descriptor.HandleMethod.Invoke(handler, [entry.EventData, CancellationToken.None]) is
                                not Task
                                task)
                            {
                                continue;
                            }

                            try
                            {
                                await task;
                                var unitOfWork = services.GetService<IUnitOfWork>();
                                if (unitOfWork != null)
                                {
                                    await unitOfWork.SaveChangesAsync(CancellationToken.None);
                                }

                                logger.LogDebug(
                                    "{TraceId}, 事件处理器 {HandlerType} 执行结束", traceId, handlerName);
                            }
                            catch (Exception e)
                            {
                                logger.LogError(e, "{TraceId}, 事件处理器 {HandlerType} 执行失败",
                                    traceId, handlerName);
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
        }, stoppingToken);
    }

    /// <summary>
    ///
    /// </summary>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    public override async Task StopAsync(CancellationToken cancellationToken)
    {
        logger.LogInformation("开始关闭本地事件服务");
        await base.StopAsync(cancellationToken);
        logger.LogInformation("关闭本地事件服务完成");
    }

    private AuditOperation CreateAuditedOperation(ISession session, string handlerType)
    {
        var auditedOperation = new AuditOperation(handlerType, null, null, null, null,
            null, null, session.TraceIdentifier);
        auditedOperation.SetCreation(session.UserId, session.UserDisplayName, DateTimeOffset.Now);
        return auditedOperation;
    }
}
