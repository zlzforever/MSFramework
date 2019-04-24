using System;

namespace MSFramework.Domain.Event
{
	/// <summary>
	/// 聚合内部事件，通过内部消息总线发布
	/// </summary>
	public interface IAggregateEvent : IDomainEvent
	{
		long Version { get; }

		Guid AggregateId { get; }
	}

	/// <summary>
	/// 聚合内部事件，通过内部消息总线发布
	/// </summary>
	public abstract class AggregateEventBase : EventBus.Event, IAggregateEvent
	{
		public long Version { get; private set; }

		public Guid AggregateId { get; private set; }

		internal void SetAggregateIdAndVersion(Guid aggregateId, long version)
		{
			AggregateId = aggregateId;
			Version = version;
		}
	}
}