using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using MSFramework.Domain;

namespace MSFramework.EntityFrameworkCore.Repository
{
	/// <summary>
	/// Read 仓储必须严格保证不会有写操作
	/// </summary>
	/// <typeparam name="TAggregateRoot"></typeparam>
	public abstract class EfReadRepository<TAggregateRoot> :
		IEfReadRepository<TAggregateRoot>
		where TAggregateRoot : AggregateRootBase
	{
		protected DbContext DbContext { get; }

		protected IQueryable<TAggregateRoot> Aggregates { get; }

		public EfReadRepository(DbContextFactory dbContextFactory)
		{
			DbContext = dbContextFactory.GetDbContext<TAggregateRoot>();
			Aggregates = DbContext.Set<TAggregateRoot>().AsNoTracking();
		}

		public virtual List<TAggregateRoot> GetAllList()
		{
			return Aggregates.ToList();
		}

		public virtual async Task<List<TAggregateRoot>> GetAllListAsync()
		{
			return await Aggregates.ToListAsync();
		}

		public virtual TAggregateRoot Get(Guid id)
		{
			return Aggregates.FirstOrDefault(x => x.Id == id);
		}

		public virtual async Task<TAggregateRoot> GetAsync(Guid id)
		{
			return await Aggregates.FirstOrDefaultAsync(x => x.Id == id);
		}

		public int Count()
		{
			return Aggregates.Count();
		}

		public async Task<int> CountAsync()
		{
			return await Aggregates.CountAsync();
		}

		public long LongCount()
		{
			return Aggregates.LongCount();
		}

		public async Task<long> LongCountAsync()
		{
			return await Aggregates.LongCountAsync();
		}
	}
}