using System;
using Microsoft.Extensions.Caching.Distributed;

namespace MSFramework.Domain
{
	public class AggregateRootDistributedCache: IAggregateRootCache
	{
		private readonly IDistributedCache _cache;
		
		public AggregateRootDistributedCache(IDistributedCache cache)
		{
			_cache = cache;
		}

		public void Set<TAggregateRoot, TAggregateRootId>(TAggregateRoot aggregateRoot, int? version = null) where TAggregateRoot : AggregateRootBase<TAggregateRoot, TAggregateRootId> where TAggregateRootId : IEquatable<TAggregateRootId>
		{
			throw new NotImplementedException();
		}

		public TAggregateRoot Get<TAggregateRoot, TAggregateRootId>(TAggregateRootId aggregateRootId) where TAggregateRoot : AggregateRootBase<TAggregateRoot, TAggregateRootId> where TAggregateRootId : IEquatable<TAggregateRootId>
		{
			throw new NotImplementedException();
		}
	}
}