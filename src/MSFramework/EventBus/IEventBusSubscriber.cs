using System;

namespace MicroserviceFramework.EventBus;

public interface IEventBusSubscriber
{
    void Subscribe<TEvent, TEventHandler>()
        where TEvent : EventBase
        where TEventHandler : class, IEventHandler<TEvent>;

    void Subscribe(Type eventHandlerType);
}
