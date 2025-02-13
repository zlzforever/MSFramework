using System;
using System.Collections.Generic;
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
            logger.LogInformation("本地事件消费服务启动");

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

                            logger.LogDebug("{TraceId}, 处理器 {HandlerType} 执行开始", traceId, handlerName);

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

                            if (descriptor.HandleMethod.Invoke(handler, [entry.EventData, stoppingToken]) is not Task
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
                                    await unitOfWork.SaveChangesAsync(stoppingToken);
                                }

                                logger.LogDebug(
                                    "{TraceId}, 处理器 {HandlerType} 执行结束", traceId, handlerName);
                            }
                            catch (Exception e)
                            {
                                logger.LogError(e, "{TraceId}, 处理器 {HandlerType} 执行失败",
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


    private AuditOperation CreateAuditedOperation(ISession session, string handlerType)
    {
        var auditedOperation = new AuditOperation(handlerType, null, null, null, null,
            null, null, session.TraceIdentifier);
        auditedOperation.SetCreation(session.UserId, session.UserDisplayName, DateTimeOffset.Now);
        return auditedOperation;
    }
}
