using System;
using System.Linq;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace MicroserviceFramework.EventBus;

public class DependencyInjectionEventBusSubscriber : IEventBusSubscriber
{
    private readonly IServiceCollection _services;

    public DependencyInjectionEventBusSubscriber(IServiceCollection services)
    {
        _services = services;
    }

    public void Subscribe<TEvent, TEventHandler>() where TEvent : EventBase
        where TEventHandler : class, IEventHandler<TEvent>
    {
        _services.AddScoped<IEventHandler<TEvent>, TEventHandler>();
        EventHandlerDescriptorManager.Register(typeof(TEvent), typeof(IEventHandler<TEvent>));
    }

    public void Subscribe(Type handlerType)
    {
        var serviceTypes = handlerType
            .GetInterfaces()
            .Where(i => i.IsGenericType && i.GetGenericTypeDefinition() == typeof(IEventHandler<>));

        foreach (var serviceType in serviceTypes)
        {
            var eventType = serviceType.GetGenericArguments()[0];
            _services.TryAddScoped(handlerType);
            EventHandlerDescriptorManager.Register(eventType, handlerType);
        }
    }
}
