using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;
using MSFramework.Domain;
using MSFramework.Domain.Repository;

namespace MSFramework.EntityFrameworkCore.Repository
{
	public interface IEfReadRepository<TAggregateRoot> : IReadRepository<TAggregateRoot>
		where TAggregateRoot : AggregateRootBase
	{
		/// <summary>
		/// Used to get a IQueryable that is used to retrieve entities from entire table.
		/// </summary>
		/// <returns>IQueryable to be used to select entities from database</returns>
		IQueryable<TAggregateRoot> Aggregates { get; }
	}
}