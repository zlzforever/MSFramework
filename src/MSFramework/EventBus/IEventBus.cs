using System.Threading.Tasks;

namespace MSFramework.EventBus
{
    public interface IEventBus
    {
        Task PublishAsync(Event @event);

        void Subscribe<T, TH>()
            where T : Event
            where TH : IEventHandler<T>;

        void Subscribe<TH>(string eventName)
            where TH : IDynamicEventHandler;

        void Unsubscribe<TH>(string eventName)
            where TH : IDynamicEventHandler;

        void Unsubscribe<T, TH>()
            where TH : IEventHandler<T>
            where T : Event;
    }
}