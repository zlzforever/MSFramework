using Microsoft.EntityFrameworkCore;
using MSFramework.Domain;

namespace MSFramework.EntityFrameworkCore
{
	public abstract class EfRepository<TAggregateRoot> where TAggregateRoot : AggregateRootBase
	{
		protected DbContext DbContext { get; }

		protected DbSet<TAggregateRoot> AggregateRoots { get; }

		protected EfRepository(DbContextFactory dbContextFactory)
		{
			DbContext = dbContextFactory.GetDbContext<TAggregateRoot>();
			AggregateRoots = DbContext.Set<TAggregateRoot>();
		}
	}
}