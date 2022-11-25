using System;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace MicroserviceFramework.EventBus;

internal class DefaultEventExecutor : IEventExecutor
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
            var @event = Defaults.JsonHelper.Deserialize(eventData, subscription.EventType);

            // 每次执行是独立的 scope
            var scope = _serviceProvider.CreateAsyncScope();

            var handler = scope.ServiceProvider.GetService(subscription.EventHandlerType);
            if (handler == null)
            {
                await scope.DisposeAsync();
                continue;
            }

            if (subscription.MethodInfo.Invoke(handler,
                    new[] { @event }) is Task task)
            {
                task.ContinueWith(_ =>
                {
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
