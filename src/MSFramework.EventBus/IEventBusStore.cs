using System;
using System.Collections.Generic;

namespace MSFramework.EventBus
{
    public interface IEventBusSubscriptionStore
    {
        bool IsEmpty { get; }

        event EventHandler<string> OnEventRemoved;

        void AddSubscription<TH>(string eventName)
            where TH : IDynamicEventHandler;

        void AddSubscription<T, TH>()
            where T : class, IEvent
            where TH : IEventHandler<T>;

        void RemoveSubscription<T, TH>()
            where T : class, IEvent
            where TH : IEventHandler<T>;

        void RemoveSubscription<TH>(string eventName)
            where TH : IDynamicEventHandler;

        bool ContainSubscription<T>() where T : class, IEvent;

        bool ContainSubscription(string eventName);

        Type GetEventType(string eventName);

        void Clear();

        IEnumerable<SubscriptionInfo> GetHandlers<T>() where T : class, IEvent;

        IEnumerable<SubscriptionInfo> GetHandlers(string eventName);

        string GetEventKey<T>() where T : class, IEvent;

        string GetEventKey(Type eventType);
    }
}