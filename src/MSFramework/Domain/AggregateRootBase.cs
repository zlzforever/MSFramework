using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using MediatR;
using MSFramework.Core;
using MSFramework.Domain.Entity;
using MSFramework.EventBus;

namespace MSFramework.Domain
{
	/// <summary>
	/// 本地领域事件: 聚合内部事件(用于更改数据，需要保存到 ES)直接调用，聚合互通事件(不需要保存到 ES)
	/// 外部领域事件: 通过分布式 EventBus 发布到消息队列中, 在 DbContext 中处理
	/// </summary>
	/// <typeparam name="TAggregateId"></typeparam>
	[Serializable]
	public abstract class AggregateRootBase<TAggregateId> :
		EntityBase<TAggregateId>,
		IAggregateRoot<TAggregateId>
		where TAggregateId : IEquatable<TAggregateId>
	{
		public const long NewAggregateVersion = -1;

		private readonly ICollection<IDomainEvent> _uncommittedDistributedEvents =
			new LinkedList<IDomainEvent>();

		private readonly ICollection<IDomainEvent> _uncommittedLocalEvents =
			new LinkedList<IDomainEvent>();

		private readonly ICollection<AggregateEvent<TAggregateId>> _aggregateEvents =
			new LinkedList<AggregateEvent<TAggregateId>>();

		protected AggregateRootBase()
		{
		}

		protected AggregateRootBase(TAggregateId id) : base(id)
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

		public string IdAsString() => Id.ToString();

		protected void ApplyAggregateEvent(AggregateEvent<TAggregateId> e)
		{
			ApplyAggregateEvent(e, true);
		}

		private void ApplyAggregateEvent(AggregateEvent<TAggregateId> e, bool isNew)
		{
			lock (_aggregateEvents)
			{
				((dynamic) this).Apply(e);
				if (isNew)
				{
					_aggregateEvents.Add(e);
				}
				else
				{
					Id = e.AggregateId;
					Version++;
				}
			}
		}

		#endregion

		#region  Distributred domain events

		public void ClearDistributedDomainEvent()
			=> _uncommittedDistributedEvents.Clear();

		public IEnumerable<IDomainEvent> GetDistributedDomainEvent()
			=> _uncommittedDistributedEvents.AsEnumerable();

		public void RegisterDistributedDomainEvent(IDomainEvent @event)
		{
			_uncommittedDistributedEvents.Add(@event);
		}

		#endregion

		#region  Local domain events

		public void ClearLocalDomainEvent()
			=> _uncommittedLocalEvents.Clear();

		public IEnumerable<IDomainEvent> GetLocalDomainEvent()
			=> _uncommittedLocalEvents.AsEnumerable();

		public void RegisterLocalDomainEvent(IDomainEvent @event)
		{
			_uncommittedLocalEvents.Add(@event);
		}

		#endregion

		public void LoadFromHistory(IEnumerable<AggregateEvent<TAggregateId>> history)
		{
			foreach (var e in history)
			{
				if (e.Version != Version + 1)
					throw new MSFrameworkException(e.Id.ToString());
				ApplyAggregateEvent(e, true);
			}
		}
	}
}