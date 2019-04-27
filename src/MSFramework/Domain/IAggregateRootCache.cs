using System;

namespace MSFramework.Domain
{
	public interface IAggregateRootCache
	{
		void Set<TAggregateRoot, TAggregateRootId>(TAggregateRoot aggregateRoot, int? version = null)
			where TAggregateRoot : AggregateRootBase<TAggregateRoot, TAggregateRootId>
			where TAggregateRootId : IEquatable<TAggregateRootId>;

		TAggregateRoot Get<TAggregateRoot, TAggregateRootId>(TAggregateRootId aggregateRootId)
			where TAggregateRoot : AggregateRootBase<TAggregateRoot, TAggregateRootId>
			where TAggregateRootId : IEquatable<TAggregateRootId>;
	}
}