using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using MediatR;
using MSFramework.Core;
using MSFramework.Domain.Entity;

namespace MSFramework.Domain
{
	[Serializable]
	public abstract class AggregateRootBase<TKey> : EntityBase<TKey>, IAggregateRoot, IHasConcurrencyStamp
		where TKey : IEquatable<TKey>
	{
		private ICollection<INotification> _domainEvents;

		public virtual string ConcurrencyStamp { get; set; } = CombGuid.NewGuid().ToString("N");

		public virtual IEnumerable<INotification> GetDomainEvents()
		{
			return _domainEvents;
		}

		protected AggregateRootBase()
		{
		}

		protected AggregateRootBase(TKey id) : base(id)
		{
		}

		protected virtual void AddDomainEvent(INotification @event)
		{
			_domainEvents = _domainEvents ?? new Collection<INotification>();
			_domainEvents.Add(@event);
		}

		protected virtual void RemoveDomainEvent(INotification @event)
		{
			if (_domainEvents.Contains(@event))
			{
				_domainEvents.Remove(@event);
			}
		}

		public virtual void ClearDomainEvents()
		{
			_domainEvents.Clear();
		}
	}
}