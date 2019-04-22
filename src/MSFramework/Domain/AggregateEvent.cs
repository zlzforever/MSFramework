using System;
using MSFramework.EventBus;

namespace MSFramework.Domain
{
	/// <summary>
	/// 聚合内部事件，通过内部消息总线发布
	/// </summary>
	public interface IAggregateEvent : IDomainEvent, IAggregateId
	{
		long Version { get; }
	}

	/// <summary>
	/// 聚合内部事件，通过内部消息总线发布
	/// </summary>
	public abstract class AggregateEvent<TAggregateId> : Event, IAggregateEvent
		where TAggregateId : IEquatable<TAggregateId>
	{
		public long Version { get; }

		public TAggregateId AggregateId { get; }

		public string IdAsString() => AggregateId.ToString();

		protected AggregateEvent(TAggregateId aggregateId, long version = -1)
		{
			Version = version;
			AggregateId = aggregateId;
		}
	}
}