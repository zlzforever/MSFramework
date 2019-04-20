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
	[Serializable]
	public abstract class AggregateRootBase<TAggregateId> :
		EntityBase<TAggregateId>,
		IAggregateRoot<TAggregateId>
		where TAggregateId : IEquatable<TAggregateId>
	{
		public const long NewAggregateVersion = -1;

		private readonly ICollection<IDomainEvent<TAggregateId>> _uncommittedEvents =
			new LinkedList<IDomainEvent<TAggregateId>>();

		private long _version = NewAggregateVersion;

		protected AggregateRootBase()
		{
		}

		protected AggregateRootBase(TAggregateId id) : base(id)
		{
		}

		public long Version => _version;

		public void ClearUncommittedEvents()
			=> _uncommittedEvents.Clear();

		public IEnumerable<IDomainEvent> GetUncommittedEvents()
			=> _uncommittedEvents.AsEnumerable();

		public void AddEvent(IDomainEvent<TAggregateId> @event)
		{
			@event.SetAggregateIdAndVersion(Equals(Id, default(TAggregateId)) ? @event.AggregateId : Id, _version + 1);
			_uncommittedEvents.Add(@event);
		}
	}
}