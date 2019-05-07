using System.Threading.Tasks;

namespace MSFramework.EventBus
{
	public interface IEventHandler<in TEvent>
		where TEvent : class, IEvent
	{
		Task Handle(TEvent @event);
	}
}