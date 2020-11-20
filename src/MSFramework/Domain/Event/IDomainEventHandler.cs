using System;
using System.Threading.Tasks;

namespace MicroserviceFramework.Domain.Event
{
	public interface IDomainEventHandler<in TEvent> : IDisposable
		where TEvent : DomainEvent
	{
		Task HandleAsync(TEvent @event);
	}
}