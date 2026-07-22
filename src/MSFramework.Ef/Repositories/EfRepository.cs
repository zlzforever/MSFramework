using System;
using System.Linq;
using System.Threading.Tasks;
using MicroserviceFramework.Domain;
using Microsoft.EntityFrameworkCore;

namespace MicroserviceFramework.Ef.Repositories;

/// <summary>
/// EF Core 仓储基类，提供聚合根的增删改查基础实现
/// </summary>
/// <typeparam name="TEntity">聚合根实体类型</typeparam>
/// <typeparam name="TKey">实体主键类型</typeparam>
public class EfRepository<TEntity, TKey> : IRepository<TEntity, TKey>, IEfRepository
    where TEntity : class, IAggregateRoot<TKey> where TKey : IEquatable<TKey>
{
    private readonly DbSet<TEntity> _dbSet;
    private readonly DbContextBase _dbContext;

    /// <summary>
    /// 获取可查询的实体集合，默认包含第一级导航属性
    /// </summary>
    protected virtual IQueryable<TEntity> Store
    {
        get
        {
            if (field != null)
            {
                return field;
            }

            var queryable = BuildQueryable(_dbSet);
            field = queryable;

            return field;
        }
    }

    /// <summary>
    /// 获取原始 DbSet 实例
    /// </summary>
    protected DbSet<TEntity> DbSet => _dbSet;

    /// <summary>
    /// 获取当前 DbContext 实例
    /// </summary>
    public DbContext DbContext => _dbContext;

    /// <summary>
    /// 获取或设置是否启用查询拆分行为，null 时使用全局设置
    /// </summary>
    public bool? UseQuerySplittingBehavior { get; init; }

    /// <summary>
    /// 初始化 EfRepository 实例
    /// </summary>
    /// <param name="dbContextFactory">数据库上下文工厂</param>
    public EfRepository(DbContextFactory dbContextFactory)
    {
        _dbContext = dbContextFactory.GetDbContext<TEntity>();
        _dbSet = _dbContext.Set<TEntity>();
    }

    /// <summary>
    /// 若 UseQuerySplittingBehavior 为空，则使用全局设置，默认是 SingleQuery
    /// 建议 2 个或以上的 1:N 关系则使用 SplitQuery 来避免笛卡尔积爆炸，其它情况使用 SingleQuery
    /// 即其他情况使用默认配置，仅在聚合根有较多 1:N 的关系时重载 UseQuerySplittingBehavior = true 来优化查询
    /// 默认会 include 第一级导航属性
    /// </summary>
    /// <param name="dbSet"></param>
    /// <returns></returns>
    protected virtual IQueryable<TEntity> BuildQueryable(DbSet<TEntity> dbSet)
    {
        var queryable = dbSet.AsQueryable();
        var navigations = dbSet.EntityType.GetNavigations();
        queryable = navigations.Aggregate(queryable, (current, navigation) => current.Include(navigation.Name));

        return !UseQuerySplittingBehavior.HasValue ? queryable :
            UseQuerySplittingBehavior.Value ? queryable.AsSplitQuery() : queryable.AsSingleQuery();
    }

    /// <summary>
    /// 根据主键查找实体
    /// </summary>
    /// <param name="id">实体主键</param>
    /// <returns>匹配的实体，未找到则返回 null</returns>
    public virtual TEntity Find(TKey id)
    {
        return Store.FirstOrDefault(x => x.Id.Equals(id));
    }

    /// <summary>
    /// 异步根据主键查找实体
    /// </summary>
    /// <param name="id">实体主键</param>
    /// <returns>匹配的实体，未找到则返回 null</returns>
    public virtual async Task<TEntity> FindAsync(TKey id)
    {
        return await Store.FirstOrDefaultAsync(x => x.Id.Equals(id));
    }

    /// <summary>
    /// 添加新实体到仓储
    /// </summary>
    /// <param name="entity">要添加的实体</param>
    public virtual void Add(TEntity entity)
    {
        _dbSet.Add(entity);
    }

    /// <summary>
    /// 异步添加新实体到仓储
    /// </summary>
    /// <param name="entity">要添加的实体</param>
    public virtual async Task AddAsync(TEntity entity)
    {
        await _dbSet.AddAsync(entity);
    }

    /// <summary>
    /// 从仓储中删除实体
    /// </summary>
    /// <param name="entity">要删除的实体</param>
    public virtual void Delete(TEntity entity)
    {
        _dbSet.Remove(entity);
    }

    /// <summary>
    /// 异步从仓储中删除实体
    /// </summary>
    /// <param name="entity">要删除的实体</param>
    /// <returns>异步任务</returns>
    public virtual Task DeleteAsync(TEntity entity)
    {
        Delete(entity);
        return Task.CompletedTask;
    }

    /// <summary>
    /// 根据主键删除实体
    /// </summary>
    /// <param name="id">要删除的实体主键</param>
    public virtual void Delete(TKey id)
    {
        var entity = Find(id);
        if (entity != null)
        {
            Delete(entity);
        }
    }

    /// <summary>
    /// 异步根据主键删除实体
    /// </summary>
    /// <param name="id">要删除的实体主键</param>
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

    /// <summary>
    /// 获取原始 DbSet 实例，用于高级查询操作
    /// </summary>
    /// <returns>DbSet 实例</returns>
    public DbSet<TEntity> GetDbSet()
    {
        return _dbSet;
    }
}
