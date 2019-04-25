using System;
using System.Collections.Generic;
using MSFramework.Domain.Event;

namespace MSFramework.Domain
{
	public interface IAggregateRoot
	{
		void RegisterDomainEvent(IDomainEvent @event);

		IEnumerable<IDomainEvent> GetDomainEvents();

		void ClearDomainEvents();

		
	}

	public interface IAggregateRoot<out TAggregateRootId> : IEventSourcingAggregate, IAggregateRoot
		where TAggregateRootId : IEquatable<TAggregateRootId>
	{
		TAggregateRootId Id { get; }
		
		void LoadFromHistory(params IAggregateEvent[] histories);
	}
}