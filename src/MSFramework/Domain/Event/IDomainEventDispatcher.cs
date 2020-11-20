using System;
using System.Threading.Tasks;

namespace MicroserviceFramework.Domain.Event
{
	public interface IDomainEventDispatcher : IDisposable
	{
		Task DispatchAsync(DomainEvent @event);
	}
}