using System;
using System.Collections.Generic;
using System.Linq;
using MSFramework.Common;
using MSFramework.Domain.Event;

namespace MSFramework.Domain
{
	/// <summary>
	/// 本地领域事件: 聚合内部事件(用于更改数据，需要保存到 ES)直接调用，聚合互通事件(不需要保存到 ES)
	/// 外部领域事件: 通过分布式 EventBus 发布到消息队列中, 在 DbContext 中处理
	/// </summary>
	[Serializable]
	public abstract class AggregateRootBase :
		EntityBase<Guid>,
		IAggregateRoot
	{
		public const long NewAggregateVersion = -1;

		private readonly ICollection<IDomainEvent> _uncommittedEvents =
			new LinkedList<IDomainEvent>();

		private readonly ICollection<AggregateEventBase> _aggregateEvents =
			new LinkedList<AggregateEventBase>();

		protected AggregateRootBase() : base(CombGuid.NewGuid())
		{
		}

		protected AggregateRootBase(Guid id) : base(id)
		{
		}

		#region AggregateEvent

		public long Version { get; protected set; } = NewAggregateVersion;

		public IEnumerable<IAggregateEvent> GetAggregateEvents() =>
			_aggregateEvents.AsEnumerable();


		public void ClearAggregateEvents()
		{
			_aggregateEvents.Clear();
		}

		protected void ApplyAggregateEvent(AggregateEventBase aggregateEvent)
		{
			Version++;

			// TODO: 是否需要 lock?
			aggregateEvent.SetAggregateIdAndVersion(Id, Version);
			PrivateReflectionDynamicObject.WrapObjectIfNeeded(this).Apply(aggregateEvent);
			_aggregateEvents.Add(aggregateEvent);
		}

		#endregion

		#region  Domain events

		public void ClearDomainEvents()
			=> _uncommittedEvents.Clear();

		public IEnumerable<IDomainEvent> GetDomainEvents()
			=> _uncommittedEvents.AsEnumerable();

		public void RegisterDomainEvent(IDomainEvent @event)
		{
			_uncommittedEvents.Add(@event);
		}

		#endregion

		public void LoadFromHistory(params IAggregateEvent[] histories)
		{
			foreach (var @event in histories)
			{
				if (@event.Version != Version + 1)
				{
					throw new MSFrameworkException(@event.Id.ToString());
				}

				var aggregateEvent = (AggregateEventBase) @event;
				Id = aggregateEvent.AggregateId;
				Version = aggregateEvent.Version;
				PrivateReflectionDynamicObject.WrapObjectIfNeeded(this).Apply(aggregateEvent);
			}
		}
	}
}