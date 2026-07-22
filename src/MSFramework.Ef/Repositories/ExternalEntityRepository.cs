using System;
using System.Collections.Concurrent;
using System.Linq;
using MicroserviceFramework.Domain;
using MicroserviceFramework.Utils;
using Microsoft.EntityFrameworkCore;

namespace MicroserviceFramework.Ef.Repositories;

/// <summary>
/// 外部实体仓储，用于缓存外部实体的只读查询结果
/// </summary>
public class ExternalEntityRepository<TEntity, TKey>
    : IExternalEntityRepository<TEntity, TKey>
    where TEntity : ExternalEntity<TKey>
    where TKey : IEquatable<TKey>
{
    private readonly ConcurrentDictionary<object, dynamic> _cache = new();

    /// <summary>
    /// 获取只读查询集合（AsNoTracking）
    /// </summary>
    protected IQueryable<TEntity> Store { get; }

    /// <summary>
    /// 初始化 ExternalEntityRepository 实例
    /// </summary>
    /// <param name="dbContextFactory">数据库上下文工厂</param>
    public ExternalEntityRepository(DbContextFactory dbContextFactory)
    {
        var dbContext = dbContextFactory.GetDbContext<TEntity>();
        Store = dbContext.Set<TEntity>().AsNoTracking();
    }

    /// <summary>
    /// 加载外部实体，通过工厂方法创建并缓存
    /// </summary>
    /// <param name="factory">实体工厂委托</param>
    /// <returns>缓存的实体</returns>
    public TEntity Load(Func<TEntity> factory)
    {
        Check.NotNull(factory, nameof(factory));
        var item = factory();
        return Load(item);
    }

    /// <summary>
    /// 加载并缓存外部实体
    /// </summary>
    /// <param name="entity">要缓存的外部实体</param>
    /// <returns>缓存的实体</returns>
    public TEntity Load(TEntity entity)
    {
        Check.NotNull(entity, nameof(entity));
        var key = entity.Id;
        return _cache.GetOrAdd(key, _ => entity);
    }

    /// <summary>
    /// 释放缓存资源
    /// </summary>
    public void Dispose()
    {
        _cache.Clear();
    }
}
