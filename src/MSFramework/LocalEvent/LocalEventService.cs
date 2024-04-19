using System;
using System.Threading;
using System.Threading.Channels;
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

public class LocalEventService(
    IServiceProvider serviceProvider,
    ILogger<LocalEventService> logger,
    IOptions<LocalEventOptions> localEventOptions)
    : BackgroundService
{
    internal static readonly Channel<(ISession Session, EventBase EventData)>
        EventChannel =
            Channel.CreateUnbounded<(ISession, EventBase)>();

    protected override Task ExecuteAsync(CancellationToken stoppingToken)
    {
        return Task.Factory.StartNew(async () =>
        {
            logger.LogInformation("本地消费服务启动");

            while (await EventChannel.Reader.WaitToReadAsync(stoppingToken))
            {
                while (EventChannel.Reader.TryRead(out var state))
                {
                    try
                    {
                        var traceId = state.Session == null
                            ? ObjectId.GenerateNewId().ToString()
                            : state.Session.TraceIdentifier;
                        var eventType = state.EventData.GetType();

                        var descriptors = EventHandlerStore.Get(eventType);

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
                            if (state.Session != null)
                            {
                                session?.Load(state.Session);
                            }

                            if (localEventOptions.Value.EnableAuditing)
                            {
                                var unitOfWork = services.GetService<IUnitOfWork>();
                                unitOfWork?.SetAuditOperationFactory(() =>
                                    CreateAuditedOperation(session, handlerName));
                            }

                            if (descriptor.HandleMethod.Invoke(handler, [state.EventData, stoppingToken]) is not Task
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
                            MicroserviceFramework.Defaults.JsonSerializer.Serialize(state));
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
