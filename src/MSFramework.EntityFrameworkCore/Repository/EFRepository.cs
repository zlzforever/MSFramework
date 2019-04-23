using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;
using Castle.Core.Internal;
using Microsoft.EntityFrameworkCore;
using MSFramework.Domain;
using MSFramework.Domain.Entity;
using Z.EntityFramework.Plus;

namespace MSFramework.EntityFrameworkCore.Repository
{
	/// <summary>
	/// 实体数据存储操作类
	/// </summary>
	/// <typeparam name="TEntity">实体类型</typeparam>
	/// <typeparam name="TKey">实体主键类型</typeparam>
	public class EfRepository<TEntity, TKey> : IEfRepository<TEntity, TKey>
		where TEntity : EntityBase<TKey>
		where TKey : IEquatable<TKey>
	{
		private readonly DbContext _dbContext;

		public DbContext Context => _dbContext;

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

		public EfRepository(DbContextFactory dbContextFactory)
		{
			_dbContext = dbContextFactory.GetDbContext<TEntity>();
			Table = _dbContext.Set<TEntity>();
		}

		public IQueryable<TEntity> GetAll()
		{
			return GetAllIncluding();
		}

		public IQueryable<TEntity> GetAllIncluding(params Expression<Func<TEntity, object>>[] propertySelectors)
		{
			var query = Table.AsQueryable();

			if (!propertySelectors.IsNullOrEmpty())
			{
				foreach (var propertySelector in propertySelectors)
				{
					query = query.Include(propertySelector);
				}
			}

			return query;
		}

		public List<TEntity> GetAllList()
		{
			return GetAll().ToList();
		}

		public async Task<List<TEntity>> GetAllListAsync()
		{
			return await GetAll().ToListAsync();
		}

		public List<TEntity> GetAllList(Expression<Func<TEntity, bool>> predicate)
		{
			return GetAll().Where(predicate).ToList();
		}

		public async Task<List<TEntity>> GetAllListAsync(Expression<Func<TEntity, bool>> predicate)
		{
			return await GetAll().Where(predicate).ToListAsync();
		}

		public async Task<TEntity> FirstOrDefaultAsync(Expression<Func<TEntity, bool>> predicate)
		{
			return await GetAll().FirstOrDefaultAsync(predicate);
		}

		public TEntity FirstOrDefault(Expression<Func<TEntity, bool>> predicate)
		{
			return GetAll().FirstOrDefault(predicate);
		}

		public TEntity Get(TKey id)
		{
			return GetAll().FirstOrDefault(i => i.Id.Equals(id));
		}

		public async virtual Task<TEntity> GetAsync(TKey id)
		{
			return await GetAll().FirstOrDefaultAsync(i => i.Id.Equals(id));
		}

		public TEntity Insert(TEntity entity)
		{
			return Table.Add(entity).Entity;
		}

		public async virtual Task<TEntity> InsertAsync(TEntity entity)
		{
			return (await Table.AddAsync(entity)).Entity;
		}

		public TEntity Update(TEntity entity)
		{
			AttachIfNot(entity);
			_dbContext.Entry(entity).State = EntityState.Modified;
			return entity;
		}

		public virtual Task<TEntity> UpdateAsync(TEntity entity)
		{
			entity = Update(entity);
			return Task.FromResult(entity);
		}

		public void Delete(TEntity entity)
		{
			AttachIfNot(entity);
			Table.Remove(entity);
		}

		public virtual Task DeleteAsync(TEntity entity)
		{
			Delete(entity);
			return Task.FromResult(0);
		}

		public void Delete(TKey id)
		{
			var entity = GetFromChangeTrackerOrNull(id);
			if (entity != null)
			{
				Delete(entity);
				return;
			}

			entity = Get(id);
			if (entity != null)
			{
				Delete(entity);
			}

			//Could not found the entity, do nothing.
		}

		public virtual Task DeleteAsync(TKey id)
		{
			Delete(id);
			return Task.FromResult(0);
		}

		public void Delete(Expression<Func<TEntity, bool>> predicate)
		{
			GetAll().Where(predicate).Delete();
		}

		public async Task DeleteAsync(Expression<Func<TEntity, bool>> predicate)
		{
			await GetAll().Where(predicate).DeleteAsync();
		}

		public int Count()
		{
			return GetAll().Count();
		}

		public Task<int> CountAsync()
		{
			return Task.FromResult(Count());
		}

		public int Count(Expression<Func<TEntity, bool>> predicate)
		{
			return GetAll().Where(predicate).Count();
		}

		public Task<int> CountAsync(Expression<Func<TEntity, bool>> predicate)
		{
			return Task.FromResult(Count(predicate));
		}

		public long LongCount()
		{
			return GetAll().LongCount();
		}

		public Task<long> LongCountAsync()
		{
			return Task.FromResult(LongCount());
		}

		public long LongCount(Expression<Func<TEntity, bool>> predicate)
		{
			return GetAll().Where(predicate).LongCount();
		}

		public Task<long> LongCountAsync(Expression<Func<TEntity, bool>> predicate)
		{
			return Task.FromResult(LongCount(predicate));
		}

		protected virtual void AttachIfNot(TEntity entity)
		{
			var entry = _dbContext.ChangeTracker.Entries().FirstOrDefault(ent => ent.Entity == entity);
			if (entry != null)
			{
				return;
			}

			Table.Attach(entity);
		}

		private TEntity GetFromChangeTrackerOrNull(TKey id)
		{
			var entry = _dbContext.ChangeTracker.Entries()
				.FirstOrDefault(
					ent =>
						ent.Entity is TEntity &&
						EqualityComparer<TKey>.Default.Equals(id, (ent.Entity as TEntity).Id)
				);

			return entry?.Entity as TEntity;
		}
	}
}