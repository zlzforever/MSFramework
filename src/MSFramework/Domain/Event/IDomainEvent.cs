using System;
using MSFramework.EventBus;

namespace MSFramework.Domain.Event
{
	public interface IDomainEvent : IEvent
	{
		string GetAggregateRootId();

		int Version { get; }

		void SetAggregateRootIdAndVersion(object aggregateRootId, int version);
	}

	public interface IDomainEvent<TAggregateRoot, out TAggregateRootId> : IDomainEvent
		where TAggregateRoot : AggregateRootBase<TAggregateRoot, TAggregateRootId>
		where TAggregateRootId : IEquatable<TAggregateRootId>
	{
		TAggregateRootId AggregateRootId { get; }
	}
}