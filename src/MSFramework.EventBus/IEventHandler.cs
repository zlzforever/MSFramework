using System.Threading.Tasks;

namespace MicroserviceFramework.EventBus
{
	public interface IEventHandler<in TEvent>
		where TEvent : Event
	{
		Task HandleAsync(TEvent @event);
	}
}