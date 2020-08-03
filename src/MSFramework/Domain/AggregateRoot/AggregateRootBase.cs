using System;
using System.Collections.Generic;
using MSFramework.Common;
using MSFramework.Domain.Entity;
using MSFramework.Domain.Event;

namespace MSFramework.Domain.AggregateRoot
{
	[Serializable]
	public abstract class AggregateRootBase : AggregateRootBase<ObjectId>
	{
		protected AggregateRootBase(ObjectId id) : base(id)
		{
		}
	}

	[Serializable]
	public abstract class AggregateRootBase<TKey> :
		EntityBase<TKey>,
		IAggregateRoot<TKey>
		where TKey : IEquatable<TKey>
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
		
		protected AggregateRootBase(TKey id) : base(id)
		{
		}
	}
}