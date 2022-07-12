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
		protected DbSet<TEntity> DbSet { get; }

		protected EfRepository(DbContextFactory dbContextFactory)
		{
			DbContext = dbContextFactory.GetDbContext<TEntity>();
			DbSet = DbContext.Set<TEntity>();
		}

		internal DbContextBase DbContext { get; }

		public virtual void Add(TEntity entity)
		{
			DbSet.Add(entity);
		}

		public virtual async Task AddAsync(TEntity entity)
		{
			await DbSet.AddAsync(entity);
		}

		public virtual void Delete(TEntity entity)
		{
			DbSet.Remove(entity);
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
			switch (NavigationLoader)
			{
				case NavigationLoader.Load:
				{
					var entity = DbSet.Find(id);
					if (entity == null)
					{
						return null;
					}

					foreach (var navigation in DbContext.Entry(entity).Navigations)
					{
						if (!navigation.IsLoaded)
						{
							navigation.Load();
						}
					}

					return entity;
				}
				case NavigationLoader.Include:
				default:
				{
					return IncludeNavigations().FirstOrDefault(x => x.Id.Equals(id));
				}
			}
		}

		public virtual async Task<TEntity> FindAsync(TKey id)
		{
			switch (NavigationLoader)
			{
				case NavigationLoader.Load:
				{
					var entity = await DbSet.FindAsync(id);
					if (entity == null)
					{
						return null;
					}

					foreach (var navigation in DbContext.Entry(entity).Navigations)
					{
						if (!navigation.IsLoaded)
						{
							await navigation.LoadAsync();
						}
					}

					return entity;
				}
				case NavigationLoader.Include:
				default:
				{
					return await IncludeNavigations().FirstOrDefaultAsync(x => x.Id.Equals(id));
				}
			}
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

		protected virtual NavigationLoader NavigationLoader => NavigationLoader.Include;

		private IQueryable<TEntity> IncludeNavigations()
		{
			var queryable = DbSet.AsQueryable();
			var navigations = DbSet.EntityType.GetNavigations();
			foreach (var navigation in navigations)
			{
				queryable = queryable.Include(navigation.Name);
			}

			return queryable;
		}

		protected EfRepository(DbContextFactory dbContextFactory) : base(dbContextFactory)
		{
		}
	}
}