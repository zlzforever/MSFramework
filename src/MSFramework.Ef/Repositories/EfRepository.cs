using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using MSFramework.Common;
using MSFramework.Domain;

namespace MSFramework.Ef.Repositories
{
	public abstract class EfRepository<TEntity> : EfRepository<TEntity, ObjectId>
		, IRepository<TEntity>
		where TEntity : class, IAggregateRoot<ObjectId>
	{
		protected EfRepository(DbContextFactory dbContextFactory) : base(dbContextFactory)
		{
		}
	}

	public abstract class EfRepository<TEntity, TKey> : IRepository<TEntity, TKey>
		where TEntity : class, IAggregateRoot<TKey>
		where TKey : IEquatable<TKey>
	{
		public IQueryable<TEntity> CurrentSet { get; private set; }

		protected EfRepository(DbContextFactory dbContextFactory)
		{
			DbContext = dbContextFactory.GetDbContext<TEntity>();
			var isDeletionAuditedEntity = typeof(IDeletion).IsAssignableFrom(typeof(TEntity));
			CurrentSet = isDeletionAuditedEntity
				? DbContext.Set<TEntity>().Where(x => !((IDeletion) x).Deleted)
				: DbContext.Set<TEntity>();
		}

		protected DbContextBase DbContext { get; }

		public virtual TEntity Get(TKey id)
		{
			return CurrentSet.FirstOrDefault(x => x.Id.Equals(id));
		}

		public virtual async Task<TEntity> GetAsync(TKey id)
		{
			return await CurrentSet.FirstOrDefaultAsync(x => x.Id.Equals(id));
		}

		public virtual TEntity Insert(TEntity entity)
		{
			return DbContext.Set<TEntity>().Add(entity).Entity;
		}

		public virtual async Task<TEntity> InsertAsync(TEntity entity)
		{
			return (await DbContext.Set<TEntity>().AddAsync(entity)).Entity;
		}

		public virtual TEntity Update(TEntity entity)
		{
			var entry = DbContext.Entry(entity);
			entry.State = EntityState.Modified;
			return entry.Entity;
		}

		public virtual Task<TEntity> UpdateAsync(TEntity entity)
		{
			var entry = DbContext.Entry(entity);
			entry.State = EntityState.Modified;
			return Task.FromResult(entry.Entity);
		}

		public virtual TEntity Delete(TEntity entity)
		{
			DbContext.Set<TEntity>().Remove(entity);
			return entity;
		}

		public virtual Task<TEntity> DeleteAsync(TEntity entity)
		{
			DbContext.Set<TEntity>().Remove(entity);
			return Task.FromResult(entity);
		}

		public virtual TEntity Delete(TKey id)
		{
			var entity = Get(id);
			if (entity != null)
			{
				DbContext.Set<TEntity>().Remove(entity);
				return entity;
			}
			else
			{
				return null;
			}
		}

		public virtual async Task<TEntity> DeleteAsync(TKey id)
		{
			var entity = await GetAsync(id);
			if (entity != null)
			{
				DbContext.Set<TEntity>().Remove(entity);
				return entity;
			}
			else
			{
				return null;
			}
		}
	}
}