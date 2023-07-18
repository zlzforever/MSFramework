using System;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace MicroserviceFramework.EventBus;

internal class DefaultEventProcessor : IEventProcessor
{
    private readonly IServiceProvider _serviceProvider;
    private readonly ILogger<DefaultEventProcessor> _logger;

    public DefaultEventProcessor(IServiceProvider serviceProvider, ILogger<DefaultEventProcessor> logger)
    {
        _serviceProvider = serviceProvider;
        _logger = logger;
    }

    public async Task ExecuteAsync(string eventName, string eventData)
    {
        var subscriptions = EventHandlerDescriptorManager.GetOrDefault(eventName);
        if (subscriptions == null)
        {
            _logger.LogWarning("没有找到事件 {EventName} 的处理器", eventName);
            return;
        }

        var traceId = Guid.NewGuid().ToString("N");

        foreach (var subscription in subscriptions)
        {
            var handlerType = subscription.HandlerType;

            _logger.LogInformation(
                "{TraceId}, {HandlerType} start handle request {EventData}", traceId, handlerType.FullName,
                eventData);

            var @event = Defaults.JsonHelper.Deserialize(eventData, subscription.Type);

            // 每次执行是独立的 scope
            var scope = _serviceProvider.CreateAsyncScope();

            var handler = scope.ServiceProvider.GetService(handlerType);
            if (handler == null)
            {
                await scope.DisposeAsync();
                continue;
            }

            if (subscription.HandlerMethodInfo.Invoke(handler,
                    new[] { @event }) is Task task)
            {
                task.ContinueWith(t =>
                {
                    if (t.Exception == null)
                    {
                        _logger.LogInformation(
                            "{TraceId}, {HandlerType} handle success", traceId, handlerType.FullName);
                    }
                    else
                    {
                        _logger.LogError("{TraceId}, {HandlerType} handle failed: {Exception}", traceId,
                            handlerType.FullName, t.Exception);
                    }

                    (handler as IDisposable)?.Dispose();
                    scope.Dispose();
                }).ConfigureAwait(false).GetAwaiter();
            }
            else
            {
                await scope.DisposeAsync();
            }
        }
    }
}
