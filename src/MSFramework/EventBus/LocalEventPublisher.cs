using System;
using System.Collections;
using System.Reflection;
using System.Threading.Tasks;
using MicroserviceFramework.Application;
using MicroserviceFramework.Utils;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using MongoDB.Bson;

namespace MicroserviceFramework.EventBus;

internal class LocalEventPublisher(
    IServiceProvider serviceProvider,
    ILogger<LocalEventPublisher> logger)
    : IEventPublisher
{
    public void Publish<TEvent>(TEvent @event) where TEvent : EventBase
    {
        Check.NotNull(@event, nameof(@event));

        var eventType = @event.GetType();
        var metadata = eventType.GetEventMetadata();

        using var scope = serviceProvider.CreateScope();
        if (scope.ServiceProvider.GetService(metadata.ServiceType) is not IEnumerable handlers)
        {
            return;
        }

        var session = scope.ServiceProvider.GetService<ISession>();
        var traceId = session == null ? ObjectId.GenerateNewId().ToString() : session.TraceIdentifier;
        foreach (var handler in handlers)
        {
            Task.Factory.StartNew(state =>
                {
                    var wrapper = (EventWrapper)state;
                    if (wrapper.HandleMethod.Invoke(wrapper.Handler, new[] { wrapper.EventData }) is Task task)
                    {
                        return task.ContinueWith(t =>
                        {
                            if (t.Exception == null)
                            {
                                logger.LogDebug(
                                    "{TraceId}, 处理器 {HandlerType} 执行成功", traceId, wrapper.HandlerType.FullName);
                            }
                            else
                            {
                                logger.LogError(t.Exception, "{TraceId}, 处理器 {HandlerType} 执行失败",
                                    traceId, wrapper.HandlerType.FullName);
                            }

                            (handler as IDisposable)?.Dispose();
                        });
                    }

                    return Task.CompletedTask;
                }, new EventWrapper(@event, handler, metadata.HandlerInterfaceType, metadata.Method))
                .GetAwaiter();
        }
    }

    record EventWrapper(object EventData, object Handler, Type HandlerType, MethodInfo HandleMethod);
}
