using System.Threading.Tasks;

namespace MicroserviceFramework.Domain.Event
{
	public interface IEventHandler<in TEvent>
		where TEvent : DomainEvent
	{
		Task HandleAsync(TEvent @event);
	}
}