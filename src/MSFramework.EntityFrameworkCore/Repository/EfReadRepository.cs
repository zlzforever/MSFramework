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

		protected DbSet<TAggregateRoot> Table { get; }

		public EfReadRepository(DbContextFactory dbContextFactory)
		{
			DbContext = dbContextFactory.GetDbContext<TAggregateRoot>();
			Table = DbContext.Set<TAggregateRoot>();
		}

		public virtual List<TAggregateRoot> GetAllList()
		{
			return Table.AsNoTracking().ToList();
		}

		public virtual async Task<List<TAggregateRoot>> GetAllListAsync()
		{
			return await Table.AsNoTracking().ToListAsync();
		}

		public virtual TAggregateRoot Get(Guid id)
		{
			return Table.AsNoTracking().FirstOrDefault(x => x.Id == id);
		}

		public virtual async Task<TAggregateRoot> GetAsync(Guid id)
		{
			return await Table.AsNoTracking().FirstOrDefaultAsync(x => x.Id == id);
		}

		public int Count()
		{
			return Table.AsNoTracking().Count();
		}

		public async Task<int> CountAsync()
		{
			return await Table.AsNoTracking().CountAsync();
		}

		public long LongCount()
		{
			return Table.AsNoTracking().LongCount();
		}

		public async Task<long> LongCountAsync()
		{
			return await Table.AsNoTracking().LongCountAsync();
		}

		public IQueryable<TAggregateRoot> Aggregates => Table.AsNoTracking();
	}
}