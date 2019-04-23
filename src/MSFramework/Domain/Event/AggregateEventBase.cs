using System;

namespace MSFramework.Domain.Event
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
	public interface IAggregateEvent<TAggregateId> : IAggregateEvent
		where TAggregateId : IEquatable<TAggregateId>
	{
	}

	/// <summary>
	/// 聚合内部事件，通过内部消息总线发布
	/// </summary>
	public abstract class AggregateEventBase<TAggregateId> : EventBus.Event, IAggregateEvent<TAggregateId>
		where TAggregateId : IEquatable<TAggregateId>
	{
		public long Version { get; protected set; }

		public TAggregateId AggregateId { get; protected set; }

		public string IdAsString() => AggregateId.ToString();

		protected AggregateEventBase()
		{
		}

		protected AggregateEventBase(TAggregateId aggregateId, long version = -1)
		{
			Version = version;
			AggregateId = aggregateId;
		}
	}
}