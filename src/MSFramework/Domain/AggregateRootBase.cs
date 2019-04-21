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
	public interface IES<TKey> where TKey : IEquatable<TKey>
	{
		Guid Id { get; }
		TKey AggreeId { get; }
		DateTime TimeStamp { get; }
		long Version { get; set; }
	}

	//public class EventSourceEntry<TAggregateId> where TAggregateId : IEquatable<TAggregateId>
	public interface IESHandler<TA, TES, TAggregateId>
		where TA : IAggregateRoot<TAggregateId>
		where TES : IES<TAggregateId>
		where TAggregateId : IEquatable<TAggregateId>
	{
		void Handle(TES es);
	}

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

		public void AddDomainEvent(IDomainEvent<TAggregateId> @event)
		{
			@event.SetAggregateIdAndVersion(Equals(Id, default(TAggregateId)) ? @event.AggregateId : Id, _version + 1);
			_uncommittedEvents.Add(@event);
		}

		protected void ApplyEvent(IES<TAggregateId> e)
		{
			((dynamic) this).Handle(e);
		}

		public void LoadFromHistory(IEnumerable<IES<TAggregateId>> history)
		{
			foreach (var e in history)
			{
				if (e.Version != Version + 1)
					throw new MSFrameworkException(e.Id.ToString());
				((dynamic) this).Handle(this, e);
			}
		}
	}
}