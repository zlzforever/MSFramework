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
	public abstract class EfRepository<TEntity> : IRepository<TEntity> where TEntity : class, IAggregateRoot, IEntity
	{
		private readonly DbContextBase _dbContext;

		protected EfRepository(DbContextFactory dbContextFactory)
		{
			_dbContext = dbContextFactory.GetDbContext<TEntity>();
			Table = _dbContext.Set<TEntity>();
		}

		public virtual DbConnection Connection
		{
			get
			{
				var connection = _dbContext.Database.GetDbConnection();

				if (connection.State != ConnectionState.Open)
				{
					connection.Open();
				}

				return connection;
			}
		}

		public DbSet<TEntity> Table { get; }

		public IUnitOfWork UnitOfWork => _dbContext;

		public List<TEntity> GetAllList()
		{
			return Table.ToList();
		}

		public Task<List<TEntity>> GetAllListAsync()
		{
			return Table.ToListAsync();
		}

		public TEntity Get(Guid id)
		{
			return Table.Find(id);
		}

		public Task<TEntity> GetAsync(Guid id)
		{
			return Table.FindAsync(id);
		}

		public TEntity Insert(TEntity entity)
		{
			return Table.Add(entity).Entity;
		}

		public async Task<TEntity> InsertAsync(TEntity entity)
		{
			return (await Table.AddAsync(entity)).Entity;
		}

		public TEntity Update(TEntity entity)
		{
			return Table.Update(entity).Entity;
		}

		public Task<TEntity> UpdateAsync(TEntity entity)
		{
			return Task.FromResult(Update(entity));
		}

		public void Delete(TEntity entity)
		{
			Table.Remove(entity);
		}

		public Task DeleteAsync(TEntity entity)
		{
			Table.Remove(entity);
			return Task.CompletedTask;
		}

		public void Delete(Guid id)
		{
			var entity = Get(id);
			if (entity != null)
			{
				Table.Remove(entity);
			}
		}

		public async Task DeleteAsync(Guid id)
		{
			var entity = await GetAsync(id);
			if (entity != null)
			{
				Table.Remove(entity);
			}
		}
	}
}