using System;

namespace MSFramework.Domain.Event
{
	public interface IAggregateEvent
	{
		long Version { get; }

		string IdAsString { get; }
	}

	/// <summary>
	/// 聚合内部事件，通过内部消息总线发布
	/// </summary>
	public interface IAggregateEvent<TAggregateRoot, out TAggregateRootId> : IDomainEvent, IAggregateEvent
		where TAggregateRoot : AggregateRootBase<TAggregateRoot, TAggregateRootId>
		where TAggregateRootId : IEquatable<TAggregateRootId>
	{

		TAggregateRootId AggregateId { get; }
	}

	/// <summary>
	/// 聚合内部事件，通过内部消息总线发布
	/// </summary>
	public abstract class AggregateEventBase<TAggregateRoot, TAggregateRootId> : EventBus.Event,
		IAggregateEvent<TAggregateRoot, TAggregateRootId>
		where TAggregateRoot : AggregateRootBase<TAggregateRoot, TAggregateRootId>
		where TAggregateRootId : IEquatable<TAggregateRootId>
	{
		public long Version { get; private set; }

		public TAggregateRootId AggregateId { get; private set; }

		public string IdAsString => AggregateId.ToString();		

		internal void SetAggregateIdAndVersion(TAggregateRootId aggregateId, long version)
		{
			AggregateId = aggregateId;
			Version = version;
		}
	}
}