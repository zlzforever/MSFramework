using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using MSFramework.Domain;
using MSFramework.Domain.Repository;

namespace MSFramework.Ef
{
	public class EfRepository<TEntity> : IRepository<TEntity> where TEntity : class, IEntity, IAggregateRoot
	{
		protected bool IsDeletionAuditedEntity { get; }

		public EfRepository(DbContextFactory dbContextFactory)
		{
			DbContext = dbContextFactory.GetDbContext<TEntity>();
			IsDeletionAuditedEntity = typeof(IDeletionAudited).IsAssignableFrom(typeof(TEntity));
			Entities = IsDeletionAuditedEntity
				? DbContext.Set<TEntity>().Where(x => !((IDeletionAudited) x).IsDeleted)
				: DbContext.Set<TEntity>();
		}

		protected DbContextBase DbContext { get; }

		public virtual DbConnection Connection
		{
			get
			{
				var connection = DbContext.Database.GetDbConnection();

				if (connection.State != ConnectionState.Open)
				{
					connection.Open();
				}

				return connection;
			}
		}

		public IQueryable<TEntity> Entities { get; }

		public IUnitOfWork UnitOfWork => DbContext;

		public virtual List<TEntity> GetAllList()
		{
			return Entities.ToList();
		}

		public virtual Task<List<TEntity>> GetAllListAsync()
		{
			return Entities.ToListAsync();
		}

		public virtual TEntity Get(Guid id)
		{
			return Entities.FirstOrDefault(x => x.Id == id);
		}

		public virtual async Task<TEntity> GetAsync(Guid id)
		{
			return await Entities.FirstOrDefaultAsync(x => x.Id == id);
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
			return DbContext.Set<TEntity>().Update(entity).Entity;
		}

		public virtual Task<TEntity> UpdateAsync(TEntity entity)
		{
			return Task.FromResult(Update(entity));
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

		public virtual TEntity Delete(Guid id)
		{
			var entity = Get(id);
			if (entity != null)
			{
				DbContext.Set<TEntity>().Remove(entity);
			}

			return entity;
		}

		public virtual async Task<TEntity> DeleteAsync(Guid id)
		{
			var entity = await GetAsync(id);
			if (entity != null)
			{
				DbContext.Set<TEntity>().Remove(entity);
			}

			return entity;
		}
	}
}