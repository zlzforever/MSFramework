using System;
using System.Threading;
using System.Threading.Tasks;
using MicroserviceFramework.Application;
using MicroserviceFramework.Domain;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using MongoDB.Bson;

namespace MicroserviceFramework.LocalEvent;

public class EventConsumeService(IServiceProvider serviceProvider, ILogger<EventConsumeService> logger)
    : BackgroundService
{
    protected override Task ExecuteAsync(CancellationToken stoppingToken)
    {
        return Task.Factory.StartNew(async () =>
        {
            logger.LogInformation("事件消费服务启动");

            while (await EventBusImpl.EventChannel.Reader.WaitToReadAsync(stoppingToken))
            {
                while (EventBusImpl.EventChannel.Reader.TryRead(out var state))
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

                            logger.LogInformation("{TraceId}, 处理器 {HandlerType} 执行开始", traceId, handlerName);

                            using var scope = serviceProvider.CreateScope();
                            var services = scope.ServiceProvider;
                            var handler = services.GetService(descriptor.HandlerType);
                            if (handler == null)
                            {
                                return;
                            }

                            // 覆盖 session 对象
                            if (state.Session != null)
                            {
                                var session = services.GetService<ISession>();
                                session?.Override(state.Session);
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

                                logger.LogInformation(
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
}
