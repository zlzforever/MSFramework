using System;
using System.Collections.Generic;
using MSFramework.Common;
using MSFramework.Data;
using MSFramework.EventBus;

namespace MSFramework.Domain
{
	[Serializable]
	public abstract class AggregateRootBase :
		EntityBase<Guid>,
		IAggregateRoot
	{
		private readonly List<IEvent> _domainEvents =
			new List<IEvent>();

		protected AggregateRootBase() : base(Singleton<IIdGenerator>.Instance.GetNewId<Guid>())
		{
		}

		protected AggregateRootBase(Guid id) : base(id)
		{
		}


		public void AddDomainEvent(IEvent @event)
		{
			_domainEvents.Add(@event);
		}

		public bool IsTransient()
		{
			return Id == default;
		}

		public IReadOnlyCollection<IEvent> DomainEvents => _domainEvents.AsReadOnly();

		public void ClearDomainEvents() => _domainEvents.Clear();
	}
}