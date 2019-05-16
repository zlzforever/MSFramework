using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using MSFramework.DependencyInjection;

namespace MSFramework.Domain.Repository
{
	/// <summary>
	/// This interface must be implemented by all repositories to identify them by convention.
	/// Implement generic version instead of this one.
	/// </summary>
	public interface IRepository : IScopeDependency
	{
		Task AppendAsync<TAggregateRoot>(IEnumerable<TAggregateRoot> aggregateRoots)
			where TAggregateRoot : AggregateRootBase;

		Task<TAggregateRoot> GetAsync<TAggregateRoot>(Guid aggregateRootId, int? expectedVersion = null)
			where TAggregateRoot : AggregateRootBase;
	}
}