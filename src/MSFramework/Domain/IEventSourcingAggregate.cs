using System;
using System.Collections.Generic;
using MediatR;
using MSFramework.EventBus;

namespace MSFramework.Domain
{
	public interface IEventSourcingAggregate
	{
		long Version { get; }

		IEnumerable<IDomainEvent> GetUncommittedEvents();

		void ClearUncommittedEvents();
	}

	public interface IEventSourcingAggregate<TAggregateId> : IEventSourcingAggregate
		where TAggregateId : IEquatable<TAggregateId>
	{
	}
}