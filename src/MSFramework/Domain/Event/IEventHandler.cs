using System.Threading.Tasks;

namespace MSFramework.Domain.Event
{
	public interface IEventHandler
	{
	}

	public interface IEventHandler<in TEvent> : IEventHandler
		where TEvent : class, IEvent
	{
		Task HandleAsync(TEvent @event);
	}
}