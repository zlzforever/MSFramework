using System;
using System.Reflection;
using System.Threading.Tasks;
using MicroserviceFramework.Utils;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using MongoDB.Bson;

namespace MicroserviceFramework.EventBus;

internal class LocalEventPublisher : IEventPublisher
{
    private readonly IServiceProvider _serviceProvider;
    private readonly EventBusOptions _eventBusOptions;
    private readonly ILogger<LocalEventPublisher> _logger;

    public LocalEventPublisher(IServiceProvider serviceProvider, EventBusOptions eventBusOptions,
        ILogger<LocalEventPublisher> logger)
    {
        _serviceProvider = serviceProvider;
        _eventBusOptions = eventBusOptions;
        _logger = logger;
    }

    public void Publish<TEvent>(TEvent @event) where TEvent : EventBase
    {
        var name = typeof(TEvent).GetEventName();
        Publish(name, @event);
    }

    public void Publish(string name, object @event)
    {
        Check.NotNull(@event, nameof(@event));
        Check.NotNullOrEmpty(name, nameof(name));

        var subscriptions = EventHandlerDescriptorManager.GetOrDefault(name);
        if (subscriptions == null || subscriptions.Count == 0)
        {
            _logger.LogWarning("没有找到事件 {EventName} 的处理器", name);
            return;
        }

        var json = Defaults.JsonSerializer.Serialize(@event);
        var traceId = ObjectId.GenerateNewId().ToString();

        foreach (var subscription in subscriptions)
        {
            Task.Factory.StartNew(async obj =>
                {
                    var wrapper = (EventWrapper)obj;

                    using var scope = _serviceProvider.CreateScope();
                    var handler = scope.ServiceProvider.GetService(wrapper.HandlerType);
                    if (handler == null)
                    {
                        _logger.LogWarning("没有找到事件 {EventName} 的处理器", name);
                    }

                    var e = Defaults.JsonSerializer.Deserialize(wrapper.EventData, wrapper.EventType);

                    foreach (var function in _eventBusOptions.BeforeDelegates)
                    {
                        await function(scope.ServiceProvider, e);
                    }

                    if (wrapper.HandleMethod.Invoke(handler,
                            new[] { e }) is Task task)
                    {
                        task.ContinueWith(t =>
                        {
                            if (t.Exception == null)
                            {
                                _logger.LogDebug(
                                    "{TraceId}, 处理器 {HandlerType} 执行成功", wrapper.TraceId,
                                    wrapper.HandlerType.FullName);
                            }
                            else
                            {
                                _logger.LogError("{TraceId}, 处理器 {HandlerType} 执行失败， 异常: {Exception}", wrapper.TraceId,
                                    wrapper.HandlerType.FullName, t.Exception.ToString());
                            }

                            (handler as IDisposable)?.Dispose();
                        }).ConfigureAwait(false).GetAwaiter();
                    }

                    foreach (var function in _eventBusOptions.AfterDelegates)
                    {
                        await function(scope.ServiceProvider, e);
                    }
                },
                new EventWrapper
                {
                    TraceId = traceId,
                    EventData = json,
                    EventType = subscription.EventType,
                    HandlerType = subscription.HandlerType,
                    HandleMethod = subscription.HandleMethod
                }).ConfigureAwait(false);
        }
    }

    class EventWrapper
    {
        public string TraceId { get; init; }

        /// <summary>
        /// 事件数据
        /// </summary>
        public string EventData { get; init; }

        /// <summary>
        /// 事件的类型信息
        /// </summary>
        public Type EventType { get; init; }

        /// <summary>
        /// 事件处理器的类型信息
        /// </summary>
        public Type HandlerType { get; init; }

        /// <summary>
        /// 事件处理器处理事件的方法
        /// </summary>
        public MethodInfo HandleMethod { get; init; }
    }
}
