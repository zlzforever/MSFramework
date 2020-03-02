using System;
using System.Collections.Generic;
using EventBus;
using MSFramework.Domain.Entity;

namespace MSFramework.Domain
{
	[Serializable]
	public abstract class AggregateRootBase : AggregateRootBase<Guid>
	{
	}

	[Serializable]
	public abstract class AggregateRootBase<TKey> :
		EntityBase<TKey>,
		IAggregateRoot<TKey>
		where TKey : IEquatable<TKey>
	{
		private readonly List<IEvent> _domainEvents =
			new List<IEvent>();

		protected AggregateRootBase()
		{
		}

		protected AggregateRootBase(TKey id) : base(id)
		{
		}


		public void AddDomainEvent(IEvent @event)
		{
			_domainEvents.Add(@event);
		}

		public IReadOnlyCollection<IEvent> DomainEvents => _domainEvents.AsReadOnly();

		public void ClearDomainEvents() => _domainEvents.Clear();
	}
}