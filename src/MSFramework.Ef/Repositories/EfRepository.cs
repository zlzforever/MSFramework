using System;
using System.Linq;
using System.Threading.Tasks;
using MicroserviceFramework.Domain;
using Microsoft.EntityFrameworkCore;

namespace MicroserviceFramework.Ef.Repositories;

public abstract class EfRepository<TEntity> : IRepository<TEntity>, IEfRepository
    where TEntity : class, IAggregateRoot
{
    private IQueryable<TEntity> _store;
    private readonly DbContextBase _dbContext;

    protected IQueryable<TEntity> Store => _store ??= IncludeAllNavigations(DbSet);

    protected DbSet<TEntity> DbSet { get; }

    protected EfRepository(DbContextFactory dbContextFactory)
    {
        _dbContext = dbContextFactory.GetDbContext<TEntity>();
        DbSet = _dbContext.Set<TEntity>();
    }

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

    /// <summary>
    /// 若 UseQuerySplittingBehavior 为空，则使用 OnConfiguring 中的 UseQuerySplittingBehavior 全局设置，默认是 SingleQuery
    /// 建议 2 个或以上的 1:N 关系则使用 SplitQuery 来避免笛卡尔积爆炸，其它情况使用 SingleQuery
    /// 即其他情况使用默认配置，仅在聚合根有较多 1:N 的关系时重载 UseQuerySplittingBehavior = true 来优化查询
    /// </summary>
    /// <param name="dbSet"></param>
    /// <returns></returns>
    protected virtual IQueryable<TEntity> IncludeAllNavigations(DbSet<TEntity> dbSet)
    {
        var queryable = dbSet.AsQueryable();

        var navigations = dbSet.EntityType.GetNavigations();
        queryable = navigations.Aggregate(queryable, (current, navigation) => current.Include(navigation.Name));

        return !UseQuerySplittingBehavior.HasValue ? queryable :
            UseQuerySplittingBehavior.Value ? queryable.AsSplitQuery() : queryable.AsSingleQuery();
    }

    protected bool? UseQuerySplittingBehavior { get; init; }

    public DbContextBase GetDbContext()
    {
        return _dbContext;
    }
}

public abstract class EfRepository<TEntity, TKey> : EfRepository<TEntity>, IRepository<TEntity, TKey>
    where TEntity : class, IAggregateRoot<TKey> where TKey : IEquatable<TKey>
{
    public virtual TEntity Find(TKey id)
    {
        return Store.FirstOrDefault(x => x.Id.Equals(id));
    }

    public virtual async Task<TEntity> FindAsync(TKey id)
    {
        return await Store.FirstOrDefaultAsync(x => x.Id.Equals(id));
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

    // protected async Task<TEntity> LoadAllNavigationsAsync(TEntity entity)
    // {
    // 	if (entity == null)
    // 	{
    // 		return null;
    // 	}
    //
    // 	foreach (var navigation in DbContext.Entry(entity).Navigations)
    // 	{
    // 		if (!navigation.IsLoaded)
    // 		{
    // 			await navigation.LoadAsync();
    // 		}
    // 	}
    //
    // 	return entity;
    // }

    // protected TEntity LoadAllNavigations(TEntity entity)
    // {
    // 	if (entity == null)
    // 	{
    // 		return null;
    // 	}
    //
    // 	foreach (var navigation in DbContext.Entry(entity).Navigations)
    // 	{
    // 		if (!navigation.IsLoaded)
    // 		{
    // 			navigation.Load();
    // 		}
    // 	}
    //
    // 	return entity;
    // }

    protected EfRepository(DbContextFactory dbContextFactory) : base(dbContextFactory)
    {
    }
}
