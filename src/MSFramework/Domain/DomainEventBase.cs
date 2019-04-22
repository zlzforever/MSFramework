using System;
using System.Collections.Generic;
using MSFramework.Core;

namespace MSFramework.Domain
{
	public abstract class DomainEventBase<TAggregateId> : IDomainEvent,
		IEquatable<DomainEventBase<TAggregateId>>
	{
		public Guid Id { get; }

		public DateTime CreationTime { get; }

		protected DomainEventBase()
		{
			Id = CombGuid.NewGuid();
			CreationTime = DateTime.UtcNow;
		}

		public override bool Equals(object obj)
		{
			return obj is DomainEventBase<TAggregateId> o && Equals(o);
		}

		public bool Equals(DomainEventBase<TAggregateId> other)
		{
			return other != null &&
			       Id.Equals(other.Id);
		}

		public override int GetHashCode()
		{
			return 290933282 + EqualityComparer<Guid>.Default.GetHashCode(Id);
		}
	}
}