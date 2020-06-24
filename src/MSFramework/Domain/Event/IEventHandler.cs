using System.Threading.Tasks;

namespace MSFramework.Domain.Event
{
    public interface IEventHandler<in TEvent>
        where TEvent : class, IEvent
    {
        Task HandleAsync(TEvent @event);
    }
}