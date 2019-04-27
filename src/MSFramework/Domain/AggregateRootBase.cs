using System;
using System.Collections.Generic;
using MSFramework.Common;
using MSFramework.Data;
using MSFramework.Domain.Event;

namespace MSFramework.Domain
{
	[Serializable]
	public abstract class AggregateRootBase<TAggregateRoot, TAggregateRootId> :
		EntityBase<TAggregateRootId>,
		IAggregateRoot
		where TAggregateRoot : AggregateRootBase<TAggregateRoot, TAggregateRootId>
		where TAggregateRootId : IEquatable<TAggregateRootId>
	{
		private const int NewAggregateVersion = -1;

		private readonly ICollection<IDomainEvent<TAggregateRoot, TAggregateRootId>> _uncommittedEvents =
			new LinkedList<IDomainEvent<TAggregateRoot, TAggregateRootId>>();

		private int _version = NewAggregateVersion;

		protected AggregateRootBase() : base(Singleton<IIdGenerator>.Instance.GetNewId<TAggregateRootId>())
		{
		}

		protected AggregateRootBase(TAggregateRootId id) : base(id)
		{
		}

		protected void ApplyEvent(IDomainEvent<TAggregateRoot, TAggregateRootId> @event)
		{
			@event.NotNull(nameof(@event));
			if (Equals(Id, default(TAggregateRootId)))
			{
				throw new Exception("Aggregate root id cannot be null.");
			}

			_version++;
			@event.SetAggregateRootIdAndVersion(_id, _version);
			PrivateReflectionDynamicObject.WrapObjectIfNeeded(this).Apply(@event);
			_uncommittedEvents.Add(@event);
		}

		protected void ApplyEvents(IEnumerable<IDomainEvent<TAggregateRoot, TAggregateRootId>> events)
		{
			if (events == null)
			{
				throw new ArgumentNullException(nameof(events));
			}

			foreach (var @event in events)
			{
				ApplyEvent(@event);
			}
		}

		string IAggregateRoot.GetId()
		{
			return Id.ToString();
		}

		int IAggregateRoot.Version => _version;

		void IAggregateRoot.ClearChanges()
			=> _uncommittedEvents.Clear();

		IEnumerable<IDomainEvent> IAggregateRoot.GetChanges()
			=> _uncommittedEvents;

		void IAggregateRoot.LoadFromHistory(params IDomainEvent[] histories)
		{
			foreach (var @event in histories)
			{
				var aggregateEvent = (DomainEvent<TAggregateRoot, TAggregateRootId>) @event;
				if (aggregateEvent.Version != _version + 1)
				{
					throw new MSFrameworkException(aggregateEvent.Id.ToString());
				}

				_id = (TAggregateRootId) Convert.ChangeType(aggregateEvent.AggregateRootId, typeof(TAggregateRootId));
				_version = aggregateEvent.Version;
				PrivateReflectionDynamicObject.WrapObjectIfNeeded(this).Apply(aggregateEvent);
			}
		}
	}
}