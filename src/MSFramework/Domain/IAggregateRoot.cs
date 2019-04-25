using System;
using System.Collections.Generic;
using MSFramework.Domain.Event;

namespace MSFramework.Domain
{
	public interface IAggregateRoot : IEventSourcingAggregate
	{
		Guid Id { get; }

		void RegisterDomainEvent(IDomainEvent @event);

		IEnumerable<IDomainEvent> GetDomainEvents();

		void ClearDomainEvents();

		void LoadFromHistory(params IAggregateEvent[] histories);
	}
}