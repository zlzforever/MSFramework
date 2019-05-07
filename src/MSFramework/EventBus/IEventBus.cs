using System;
using System.Threading.Tasks;

namespace MSFramework.EventBus
{
    public interface IEventBus
    {
        Task PublishAsync(IEvent @event);
        
        void Subscribe<TEvent, TEventHandler>()
            where TEvent : class, IEvent
            where TEventHandler : IEventHandler<TEvent>;

        void Subscribe<TEventHandler>(string eventName)
            where TEventHandler : IDynamicEventHandler;

        void Unsubscribe<TEventHandler>(string eventName)
            where TEventHandler : IDynamicEventHandler;

        void Unsubscribe<TEvent, TEventHandler>()
            where TEvent : class, IEvent
            where TEventHandler : IEventHandler<TEvent>;
    }
}