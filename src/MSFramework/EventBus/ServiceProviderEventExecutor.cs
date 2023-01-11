using System;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace MicroserviceFramework.EventBus;

internal class ServiceProviderEventExecutor : IEventExecutor
{
    private readonly IServiceProvider _serviceProvider;
    private readonly ILogger<ServiceProviderEventExecutor> _logger;

    public ServiceProviderEventExecutor(IServiceProvider serviceProvider, ILogger<ServiceProviderEventExecutor> logger)
    {
        _serviceProvider = serviceProvider;
        _logger = logger;
    }

    public async Task ExecuteAsync(string eventName, string eventData)
    {
        var subscriptions = EventSubscriptionManager.GetOrDefault(eventName);
        if (subscriptions == null)
        {
            _logger.LogWarning($"没有找到事件 {eventName} 的处理器");
            return;
        }

        var traceId = Guid.NewGuid().ToString("N");

        foreach (var subscription in subscriptions)
        {
            var handlerType = subscription.EventHandlerType;

            _logger.LogInformation(
                $"{traceId}, {handlerType.FullName} start handle request {eventData}");

            var @event = Defaults.JsonHelper.Deserialize(eventData, subscription.EventType);

            // 每次执行是独立的 scope
            var scope = _serviceProvider.CreateAsyncScope();

            var handler = scope.ServiceProvider.GetService(handlerType);
            if (handler == null)
            {
                await scope.DisposeAsync();
                continue;
            }

            if (subscription.MethodInfo.Invoke(handler,
                    new[] { @event }) is Task task)
            {
                task.ContinueWith(t =>
                {
                    if (t.Exception == null)
                    {
                        _logger.LogInformation(
                            $"{traceId}, {handlerType.FullName} handle success");
                    }
                    else
                    {
                        _logger.LogError($"{traceId}, {handlerType.FullName} handle failed: {t.Exception}");
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
