using System.Collections.Generic;
using EventBus;

namespace MSFramework.Domain
{
	public interface IEventProvider
	{
		IReadOnlyCollection<IEvent> DomainEvents { get; }

		void AddDomainEvent(IEvent @event);

		void ClearDomainEvents();
	}
}