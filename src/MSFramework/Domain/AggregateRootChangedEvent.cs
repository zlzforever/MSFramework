using System;
using System.Collections.Generic;
using MSFramework.Common;

namespace MSFramework.Domain
{
	public interface IAggregateRootChangedEvent
	{
		string GetAggregateRootId();

		int Version { get; }

		Guid Id { get; }

		DateTime Timestamp { get; }

		string Creator { get; }

		void SetAggregateRootIdAndVersion(object aggregateRootId, int version);

		void SetCreator(string creator);
	}

	public interface IAggregateRootChangedEvent<TAggregateRoot, out TAggregateRootId> : IAggregateRootChangedEvent
		where TAggregateRoot : AggregateRootBase<TAggregateRoot, TAggregateRootId>
		where TAggregateRootId : IEquatable<TAggregateRootId>
	{
		TAggregateRootId AggregateRootId { get; }
	}
	
	[Serializable]
	public abstract class AggregateRootChangedEvent<TAggregateRoot, TAggregateRootId> :
		IAggregateRootChangedEvent<TAggregateRoot, TAggregateRootId>
		where TAggregateRoot : AggregateRootBase<TAggregateRoot, TAggregateRootId>
		where TAggregateRootId : IEquatable<TAggregateRootId>
	{
		private TAggregateRootId _aggregateRootId;
		private int _version;

		protected AggregateRootChangedEvent()
		{
			Id = CombGuid.NewGuid();
			Timestamp = DateTime.UtcNow;
		}

		public override bool Equals(object obj)
		{
			return obj is AggregateRootChangedEvent<TAggregateRoot, TAggregateRootId> o && Equals(o);
		}

		private bool Equals(AggregateRootChangedEvent<TAggregateRoot, TAggregateRootId> other)
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

		public Guid Id { get; }

		public DateTime Timestamp { get; }

		public string Creator { get; private set; }

		public void SetCreator(string creator)
		{
			Creator = creator;
		}

		public void SetAggregateRootIdAndVersion(object aggregateRootId, int version)
		{
			_aggregateRootId = (TAggregateRootId) aggregateRootId;
			_version = version;
		}
	}
	
	public abstract class DeletedEvent<TAggregateRoot, TAggregateRootId> : AggregateRootChangedEvent<TAggregateRoot, TAggregateRootId>
		where TAggregateRoot : AggregateRootBase<TAggregateRoot, TAggregateRootId>
		where TAggregateRootId : IEquatable<TAggregateRootId>
	{
	}
}