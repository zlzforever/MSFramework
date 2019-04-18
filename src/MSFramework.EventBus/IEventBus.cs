using System.Threading.Tasks;

namespace MSFramework.EventBus
{
    public interface IEventBus
    {
        Task PublishAsync(IEvent @event);

        void Subscribe<T, TH>()
            where T : class, IEvent
            where TH : IEventHandler<T>;

        void Subscribe<TH>(string eventName)
            where TH : IDynamicEventHandler;

        void Unsubscribe<TH>(string eventName)
            where TH : IDynamicEventHandler;

        void Unsubscribe<T, TH>()
            where T : class, IEvent
            where TH : IEventHandler<T>;
    }
}