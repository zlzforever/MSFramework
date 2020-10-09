using System;
using System.Collections.Generic;
using MicroserviceFramework.Domain.Event;
using MicroserviceFramework.Shared;

namespace MicroserviceFramework.Domain
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
		private List<DomainEvent> _domainEvents;

		public IReadOnlyCollection<DomainEvent> GetDomainEvents() => _domainEvents?.AsReadOnly();

		public void AddDomainEvent(DomainEvent @event)
		{
			_domainEvents ??= new List<DomainEvent>();
			_domainEvents.Add(@event);
		}

		public void RemoveDomainEvent(DomainEvent @event)
		{
			_domainEvents?.Remove(@event);
		}

		public void ClearDomainEvents() => _domainEvents?.Clear();
		
		protected AggregateRoot(TKey id) : base(id)
		{
		}
	}
}