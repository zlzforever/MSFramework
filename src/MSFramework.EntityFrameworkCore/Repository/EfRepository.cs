using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using MSFramework.Domain;
using MSFramework.Domain.Repository;

namespace MSFramework.EntityFrameworkCore.Repository
{
	/// <summary>
	/// Read 仓储必须严格保证不会有写操作
	/// </summary>
	/// <typeparam name="TAggregateRoot"></typeparam>
	/// <typeparam name="TAggregateRootId"></typeparam>
	public class EfRepository<TAggregateRoot, TAggregateRootId> :
		IRepository<TAggregateRoot, TAggregateRootId>
		where TAggregateRoot : AggregateRootBase<TAggregateRoot, TAggregateRootId>
		where TAggregateRootId : IEquatable<TAggregateRootId>

	{
		public DbContext DbContext { get; }

		public DbSet<TAggregateRoot> AggregateRoots { get; }

		public EfRepository(DbContextFactory dbContextFactory)
		{
			DbContext = dbContextFactory.GetDbContext<TAggregateRoot>();
			AggregateRoots = DbContext.Set<TAggregateRoot>();
		}

		public virtual List<TAggregateRoot> GetAllList()
		{
			return AggregateRoots.ToList();
		}

		public virtual async Task<List<TAggregateRoot>> GetAllListAsync()
		{
			return await AggregateRoots.ToListAsync();
		}

		public virtual TAggregateRoot Get(TAggregateRootId id)
		{
			return AggregateRoots.FirstOrDefault(x => x.Id.Equals(id));
		}

		public virtual async Task<TAggregateRoot> GetAsync(TAggregateRootId id)
		{
			return await AggregateRoots.FirstOrDefaultAsync(x => x.Id.Equals(id));
		}

		public int Count()
		{
			return AggregateRoots.Count();
		}

		public async Task<int> CountAsync()
		{
			return await AggregateRoots.CountAsync();
		}

		public long LongCount()
		{
			return AggregateRoots.LongCount();
		}

		public async Task<long> LongCountAsync()
		{
			return await AggregateRoots.LongCountAsync();
		}

		public virtual TAggregateRoot Insert(TAggregateRoot entity)
		{
			AggregateRoots.Add(entity);
			return entity;
		}

		public virtual async Task<TAggregateRoot> InsertAsync(TAggregateRoot entity)
		{
			await AggregateRoots.AddAsync(entity);
			return entity;
		}

		public virtual TAggregateRoot Update(TAggregateRoot entity)
		{
			AggregateRoots.Update(entity);
			return entity;
		}

		public virtual Task<TAggregateRoot> UpdateAsync(TAggregateRoot entity)
		{
			return Task.FromResult(Update(entity));
		}

		public virtual void Delete(TAggregateRoot entity)
		{
			AggregateRoots.Remove(entity);
		}

		public virtual Task DeleteAsync(TAggregateRoot entity)
		{
			Delete(entity);
			return Task.CompletedTask;
		}

		public virtual void Delete(TAggregateRootId id)
		{
			var entity = GetFromChangeTrackerOrNull(id);
			if (entity != null)
			{
				Delete(entity);
				return;
			}

			entity = AggregateRoots.FirstOrDefault(x => x.Id.Equals(id));
			if (entity != null)
			{
				Delete(entity);
			}
		}

		public virtual Task DeleteAsync(TAggregateRootId id)
		{
			Delete(id);
			return Task.FromResult(0);
		}

		private TAggregateRoot GetFromChangeTrackerOrNull(TAggregateRootId id)
		{
			var entry = DbContext.ChangeTracker.Entries()
				.FirstOrDefault(ent => ent.Entity is TAggregateRoot aggregate && aggregate.Id.Equals(id));

			return entry?.Entity as TAggregateRoot;
		}
	}
}