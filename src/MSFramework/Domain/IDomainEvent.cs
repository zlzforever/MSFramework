using System;
using MediatR;
using MSFramework.EventBus;

namespace MSFramework.Domain
{
	public interface IDomainEvent : IEvent
	{
	}

	public interface IDomainEvent<TAggregateId> : IDomainEvent
	{
		/// <summary>
		/// The identifier of the aggregate which has generated the event
		/// </summary>
		TAggregateId AggregateId { get; }

		/// <summary>
		/// The version of the aggregate when the event has been generated
		/// </summary>
		long AggregateVersion { get; }

		void SetAggregateIdAndVersion(TAggregateId aggregateId, long aggregateVersion);
	}
}