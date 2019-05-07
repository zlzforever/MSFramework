using System;
using System.Collections.Generic;

namespace MSFramework.EventBus
{
    public interface IEventBusSubscriptionStore
    {
        bool IsEmpty { get; }

        event EventHandler<string> OnEventRemoved;

        void AddSubscription<TEventHandler>(string eventName)
            where TEventHandler : IDynamicEventHandler;

        void AddSubscription<TEvent, TEventHandler>()
            where TEvent : class, IEvent
            where TEventHandler : IEventHandler<TEvent>;

        void RemoveSubscription<TEvent, TEventHandler>()
            where TEvent : class, IEvent
            where TEventHandler : IEventHandler<TEvent>;

        void RemoveSubscription<TEventHandler>(string eventName)
            where TEventHandler : IDynamicEventHandler;

        bool ContainSubscription<TEvent>() where TEvent : class, IEvent;

        bool ContainSubscription(string eventName);

        Type GetEventType(string eventName);

        void Clear();

        IEnumerable<SubscriptionInfo> GetHandlers<TEvent>() where TEvent : class, IEvent;

        IEnumerable<SubscriptionInfo> GetHandlers(string eventName);

        string GetEventKey<TEvent>() where TEvent : class, IEvent;

        string GetEventKey(Type eventType);
    }
}