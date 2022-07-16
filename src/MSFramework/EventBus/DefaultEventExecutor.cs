using System;
using System.Linq;
using System.Threading.Tasks;
using MicroserviceFramework.Serialization;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace MicroserviceFramework.EventBus;

public class DefaultEventExecutor : IEventExecutor
{
    private readonly IServiceProvider _serviceProvider;
    private readonly ILogger<DefaultEventExecutor> _logger;

    public DefaultEventExecutor(IServiceProvider serviceProvider, ILogger<DefaultEventExecutor> logger)
    {
        _serviceProvider = serviceProvider;
        _logger = logger;
    }

    public async Task ExecuteAsync(string eventName, string eventData)
    {
        var subscriptions = EventSubscriptionManager.GetOrDefault(eventName);
        if (subscriptions == null)
        {
            _logger.LogWarning($"There is no event handler for {eventName}");
            return;
        }

        foreach (var subscription in subscriptions)
        {
            // 每次执行是独立的 scope
            await using var scope = _serviceProvider.CreateAsyncScope();
            var handlers = scope.ServiceProvider.GetServices(subscription.EventHandlerType)
                .Select(x => x as IDisposable);

            foreach (var handler in handlers)
            {
                if (handler == null)
                {
                    continue;
                }

                await Task.Yield();
                var @event = Default.JsonHelper.Deserialize(eventData, subscription.EventType);
                if (subscription.MethodInfo.Invoke(handler,
                        new[] { @event }) is Task task)
                {
                    await task.ConfigureAwait(false);
                }

                handler.Dispose();
            }
        }
    }
}