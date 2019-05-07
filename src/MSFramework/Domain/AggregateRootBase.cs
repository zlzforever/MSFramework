using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using MSFramework.Common;
using MSFramework.Data;
using MSFramework.EventBus;

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

		private readonly ICollection<IAggregateRootChangedEvent<TAggregateRoot, TAggregateRootId>>
			_aggregateRootChangedEventEvents =
				new LinkedList<IAggregateRootChangedEvent<TAggregateRoot, TAggregateRootId>>();

		private readonly List<IDomainEvent> _domainEvents =
			new List<IDomainEvent>();

		private int _version = NewAggregateVersion;

		protected AggregateRootBase() : base(Singleton<IIdGenerator>.Instance.GetNewId<TAggregateRootId>())
		{
		}

		protected AggregateRootBase(TAggregateRootId id) : base(id)
		{
		}

		public int Version
		{
			get => _version;
			protected set => _version = value;
		}

		public void AddDomainEvent(IDomainEvent @event)
		{
			_domainEvents.Add(@event);
		}

		protected void ApplyChangedEvent(IAggregateRootChangedEvent<TAggregateRoot, TAggregateRootId> @event)
		{
			@event.NotNull(nameof(@event));
			if (Equals(Id, default(TAggregateRootId)))
			{
				throw new Exception("Aggregate root id cannot be null.");
			}

			_version++;
			@event.SetAggregateRootIdAndVersion(_id, _version);
			this.ToDynamic().Apply(@event);
			_aggregateRootChangedEventEvents.Add(@event);
		}

		protected void ApplyChangedEvent(
			IEnumerable<IAggregateRootChangedEvent<TAggregateRoot, TAggregateRootId>> events)
		{
			if (events == null)
			{
				throw new ArgumentNullException(nameof(events));
			}

			foreach (var @event in events)
			{
				ApplyChangedEvent(@event);
			}
		}

		string IAggregateRoot.GetId()
		{
			return Id.ToString();
		}

		void IAggregateRoot.ClearChanges()
			=> _aggregateRootChangedEventEvents.Clear();

		IEnumerable<IAggregateRootChangedEvent> IAggregateRoot.GetChanges()
			=> _aggregateRootChangedEventEvents;

		IReadOnlyCollection<IEvent> IAggregateRoot.DomainEvents => _domainEvents.AsReadOnly();

		void IAggregateRoot.ClearDomainEvents() => _domainEvents.Clear();

		void IAggregateRoot.LoadFromHistory(params IAggregateRootChangedEvent[] histories)
		{
			foreach (var @event in histories)
			{
				var aggregateEvent = (IAggregateRootChangedEvent<TAggregateRoot, TAggregateRootId>) @event;
				if (aggregateEvent.Version != _version + 1)
				{
					throw new MSFrameworkException(aggregateEvent.Id.ToString());
				}

				_id = (TAggregateRootId) Convert.ChangeType(aggregateEvent.AggregateRootId, typeof(TAggregateRootId));
				_version = aggregateEvent.Version;
				this.ToDynamic().Apply(aggregateEvent);
			}
		}
	}
}