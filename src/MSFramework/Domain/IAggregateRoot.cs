using System;
using System.Collections.Generic;
using MediatR;

namespace MSFramework.Domain
{
	public interface IAggregateRoot : IAggregateId
	{
		void RegisterLocalDomainEvent(IDomainEvent @event);

		IEnumerable<IDomainEvent> GetLocalDomainEvents();

		void ClearLocalDomainEvents();

		void RegisterDistributedDomainEvent(IDomainEvent @event);

		IEnumerable<IDomainEvent> GetDistributedDomainEvents();

		void ClearDistributedDomainEvents();
	}

	public interface IAggregateRoot<out TAggregateId>
		: IEventSourcingAggregate where TAggregateId : IEquatable<TAggregateId>
	{
		TAggregateId Id { get; }
	}
}