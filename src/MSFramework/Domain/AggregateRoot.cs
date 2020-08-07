using System;
using System.Collections.Generic;
using MSFramework.Domain.Events;
using MSFramework.Shared;

namespace MSFramework.Domain
{
	[Serializable]
	public abstract class AggregateRoot : AggregateRoot<ObjectId>
	{
		protected AggregateRoot(ObjectId id) : base(id)
		{
		}
	}

	[Serializable]
	public abstract class AggregateRoot<TKey> :
		EntityBase<TKey>,
		IAggregateRoot<TKey>
	{
		private List<IEvent> _domainEvents;

		public IReadOnlyCollection<IEvent> GetDomainEvents() => _domainEvents?.AsReadOnly();

		public void AddDomainEvent(IEvent @event)
		{
			_domainEvents ??= new List<IEvent>();
			_domainEvents.Add(@event);
		}

		public void RemoveDomainEvent(IEvent @event)
		{
			_domainEvents?.Remove(@event);
		}

		public void ClearDomainEvents() => _domainEvents?.Clear();
		
		protected AggregateRoot(TKey id) : base(id)
		{
		}
	}
}