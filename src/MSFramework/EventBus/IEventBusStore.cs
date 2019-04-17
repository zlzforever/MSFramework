using System;
using System.Collections.Generic;

namespace MSFramework.EventBus
{
    public interface IEventBusStore
    {
        bool IsEmpty { get; }

        event EventHandler<string> OnEventRemoved;

        void AddSubscription<TH>(string eventName)
            where TH : IDynamicEventHandler;

        void AddSubscription<T, TH>()
            where T : Event
            where TH : IEventHandler<T>;

        void RemoveSubscription<T, TH>()
            where T : Event
            where TH : IEventHandler<T>;

        void RemoveSubscription<TH>(string eventName)
            where TH : IDynamicEventHandler;

        bool ContainSubscription<T>() where T : Event;

        bool ContainSubscription(string eventName);

        Type GetEventType(string eventName);

        void Clear();

        IEnumerable<SubscriptionInfo> GetHandlers<T>() where T : Event;

        IEnumerable<SubscriptionInfo> GetHandlers(string eventName);

        string GetEventKey<T>();

        string GetEventKey(Type eventType);
    }
}