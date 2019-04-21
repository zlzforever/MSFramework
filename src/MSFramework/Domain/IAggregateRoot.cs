using System;
using System.Collections.Generic;
using MediatR;

namespace MSFramework.Domain
{
	public interface IAggregateRoot
	{
		IEnumerable<IDomainEvent> GetUncommittedEvents();

		void ClearUncommittedEvents();
	}
	
	public interface IAggregateRoot<TAggregateId>
		: IEventSourcingAggregate<TAggregateId> where TAggregateId : IEquatable<TAggregateId>
	{
		TAggregateId Id { get; }
		
		void AddDomainEvent(IDomainEvent<TAggregateId> @event);		
	}
}