using System.Linq;
using System.Threading.Tasks;
using MicroserviceFramework.Domain;
using MicroserviceFramework.Shared;
using Microsoft.EntityFrameworkCore;

namespace MicroserviceFramework.Ef.Repositories
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
	{
		public IQueryable<TEntity> AggregateRootSet { get; }

		protected EfRepository(DbContextFactory dbContextFactory)
		{
			DbContext = dbContextFactory.GetDbContext<TEntity>();
			var isDeletionAuditedEntity = typeof(IDeletion).IsAssignableFrom(typeof(TEntity));
			AggregateRootSet = isDeletionAuditedEntity
				? DbContext.Set<TEntity>().Where(x => !((IDeletion) x).Deleted)
				: DbContext.Set<TEntity>();
		}

		protected DbContextBase DbContext { get; }

		public virtual TEntity Get(TKey id)
		{
			return AggregateRootSet
				.FirstOrDefault(x => x.Id.Equals(id));
		}

		public virtual async Task<TEntity> GetAsync(TKey id)
		{
			return await AggregateRootSet
				.FirstOrDefaultAsync(x => x.Id.Equals(id));
		}

		public virtual TEntity Add(TEntity entity)
		{
			return DbContext.Set<TEntity>().Add(entity).Entity;
		}

		public virtual async Task<TEntity> AddAsync(TEntity entity)
		{
			return (await DbContext.Set<TEntity>().AddAsync(entity)).Entity;
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

		public virtual void Delete(TKey id)
		{
			var entity = Get(id);
			if (entity != null)
			{
				Delete(entity);
			}
		}

		public virtual async Task DeleteAsync(TKey id)
		{
			var entity = await GetAsync(id);
			if (entity != null)
			{
				Delete(entity);
			}
		}
	}
}