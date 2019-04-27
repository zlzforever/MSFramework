using System;
using System.Collections.Generic;
using MSFramework.Common;

namespace MSFramework.Domain.Event
{
	[Serializable]
	public abstract class DomainEvent<TAggregateRoot, TAggregateRootId> : EventBus.Event,
		IDomainEvent<TAggregateRoot, TAggregateRootId>
		where TAggregateRoot : AggregateRootBase<TAggregateRoot, TAggregateRootId>
		where TAggregateRootId : IEquatable<TAggregateRootId>
	{
		private TAggregateRootId _aggregateRootId;
		private int _version;

		public override bool Equals(object obj)
		{
			return obj is DomainEvent<TAggregateRoot, TAggregateRootId> o && Equals(o);
		}

		private bool Equals(DomainEvent<TAggregateRoot, TAggregateRootId> other)
		{
			return other != null &&
			       Id.Equals(other.Id);
		}

		public override int GetHashCode()
		{
			return 290933282 + EqualityComparer<Guid>.Default.GetHashCode(Id);
		}

		public string GetAggregateRootId()
		{
			return AggregateRootId.ToString();
		}

		public int Version => _version;

		public TAggregateRootId AggregateRootId => _aggregateRootId;

		public void SetAggregateRootIdAndVersion(object aggregateRootId, int version)
		{
			_aggregateRootId = (TAggregateRootId) aggregateRootId;
			_version = version;
		}
	}
}