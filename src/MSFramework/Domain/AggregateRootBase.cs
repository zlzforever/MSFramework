using System;
using System.Collections.Generic;
using MediatR;
using MSFramework.Common;
using MSFramework.Data;

namespace MSFramework.Domain
{
	[Serializable]
	public abstract class AggregateRootBase :
		EntityBase<Guid>,
		IAggregateRoot
	{
		private readonly List<INotification> _domainEvents =
			new List<INotification>();

		protected AggregateRootBase() : base(Singleton<IIdGenerator>.Instance.GetNewId<Guid>())
		{
		}

		protected AggregateRootBase(Guid id) : base(id)
		{
		}


		public void AddDomainEvent(INotification @event)
		{
			_domainEvents.Add(@event);
		}

		public bool IsTransient()
		{
			return Id == default;
		}

		public IReadOnlyCollection<INotification> DomainEvents => _domainEvents.AsReadOnly();

		public void ClearDomainEvents() => _domainEvents.Clear();
	}
}