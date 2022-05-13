using System;
using System.Linq;
using System.Threading.Tasks;
using MicroserviceFramework.Domain;
using Microsoft.EntityFrameworkCore;

namespace MicroserviceFramework.Ef.Repositories
{
	public abstract class EfRepository<TEntity> : IRepository<TEntity>
		where TEntity : class, IAggregateRoot
	{
		public IQueryable<TEntity> Store { get; }

		protected EfRepository(DbContextFactory dbContextFactory)
		{
			DbContext = dbContextFactory.GetDbContext<TEntity>();
			Store = DbContext.Set<TEntity>();
		}

		protected DbContextBase DbContext { get; }

		public virtual void Add(TEntity entity)
		{
			DbContext.Set<TEntity>().Add(entity);
		}

		public virtual async Task AddAsync(TEntity entity)
		{
			await DbContext.Set<TEntity>().AddAsync(entity);
		}

		public virtual void Delete(TEntity entity)
		{
			DbContext.Set<TEntity>().Remove(entity);
		}

		public virtual Task DeleteAsync(TEntity entity)
		{
			Delete(entity);
			return Task.CompletedTask;
		}
	}

	public abstract class EfRepository<TEntity, TKey> : EfRepository<TEntity>, IRepository<TEntity, TKey>
		where TEntity : class, IAggregateRoot<TKey> where TKey : IEquatable<TKey>
	{
		public virtual TEntity Find(TKey id)
		{
			return DbContext.Set<TEntity>().Find(id);
		}

		public virtual async Task<TEntity> FindAsync(TKey id)
		{
			return await DbContext.Set<TEntity>().FindAsync(id);
		}

		public virtual void Delete(TKey id)
		{
			var entity = Find(id);
			if (entity != null)
			{
				Delete(entity);
			}
		}

		public virtual async Task DeleteAsync(TKey id)
		{
			var entity = await FindAsync(id);
			if (entity != null)
			{
				await DeleteAsync(entity);
			}
		}

		protected EfRepository(DbContextFactory dbContextFactory) : base(dbContextFactory)
		{
		}
	}
}