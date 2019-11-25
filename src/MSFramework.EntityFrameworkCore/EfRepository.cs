using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using MSFramework.Domain;
using MSFramework.Domain.Repository;

namespace MSFramework.EntityFrameworkCore
{
	public class EfRepository<TEntity> : IRepository<TEntity> where TEntity : class, IAggregateRoot, IEntity
	{
		public EfRepository(DbContextFactory dbContextFactory)
		{
			DbContext = dbContextFactory.GetDbContext<TEntity>();
			DbSet = DbContext.Set<TEntity>();
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

		public DbSet<TEntity> DbSet { get; }

		public IUnitOfWork UnitOfWork => DbContext;

		public virtual List<TEntity> GetAllList()
		{
			return DbSet.ToList();
		}

		public virtual Task<List<TEntity>> GetAllListAsync()
		{
			return DbSet.ToListAsync();
		}

		public virtual TEntity Get(Guid id)
		{
			return DbSet.Find(id);
		}

		public virtual Task<TEntity> GetAsync(Guid id)
		{
			return DbSet.FindAsync(id);
		}

		public virtual TEntity Insert(TEntity entity)
		{
			return DbSet.Add(entity).Entity;
		}

		public virtual async Task<TEntity> InsertAsync(TEntity entity)
		{
			return (await DbSet.AddAsync(entity)).Entity;
		}

		public virtual TEntity Update(TEntity entity)
		{
			return DbSet.Update(entity).Entity;
		}

		public virtual Task<TEntity> UpdateAsync(TEntity entity)
		{
			return Task.FromResult(Update(entity));
		}

		public virtual void Delete(TEntity entity)
		{
			DbSet.Remove(entity);
		}

		public virtual Task DeleteAsync(TEntity entity)
		{
			DbSet.Remove(entity);
			return Task.CompletedTask;
		}

		public virtual void Delete(Guid id)
		{
			var entity = Get(id);
			if (entity != null)
			{
				DbSet.Remove(entity);
			}
		}

		public virtual async Task DeleteAsync(Guid id)
		{
			var entity = await GetAsync(id);
			if (entity != null)
			{
				DbSet.Remove(entity);
			}
		}
	}
}