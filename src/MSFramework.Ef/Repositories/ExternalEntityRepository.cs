using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
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
    private readonly ConcurrentDictionary<object, Lazy<TEntity>> _cache = new();

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
    /// 使用工厂方法加载外部实体。
    /// 工厂会先执行一次（缓存键在产物中才可知），缓存命中时产物被丢弃；
    /// 按主键惰性加载请使用 <see cref="Load(TKey,Func{TEntity})"/>
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
    /// 按主键惰性加载：缓存命中直接返回，未命中才执行工厂创建并缓存。
    /// 并发下同一键的工厂由 <see cref="Lazy{T}"/> 保证最多执行一次（ExecutionAndPublication）；
    /// 工厂抛异常时会移除该缓存项，下次调用可重新尝试，不会永久缓存失败结果。
    /// </summary>
    /// <param name="key">外部实体主键（缓存键）</param>
    /// <param name="factory">实体工厂委托，仅在缓存未命中时调用</param>
    /// <returns>缓存的实体</returns>
    public TEntity Load(TKey key, Func<TEntity> factory)
    {
        Check.NotNull(factory, nameof(factory));

        var lazy = _cache.GetOrAdd(key,
            _ => new Lazy<TEntity>(factory, LazyThreadSafetyMode.ExecutionAndPublication));
        try
        {
            return lazy.Value;
        }
        catch
        {
            // 工厂失败（如数据库瞬时不可用）不缓存异常，允许下次重试。
            // 必须按"键＋原 Lazy 实例"原子条件删除（ConcurrentDictionary 的
            // ICollection<KeyValuePair<K,V>>.Remove 比较 Key+Value）：并发下本线程等待失败期间，
            // 另一线程可能已用新工厂替换了该键的缓存项，按 key 无条件 TryRemove 会误删
            // 刚替换成功的条目，破坏"同一键的工厂最多执行一次"的保证
            ((ICollection<KeyValuePair<object, Lazy<TEntity>>>)_cache).Remove(
                new KeyValuePair<object, Lazy<TEntity>>(key, lazy));
            throw;
        }
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
        return _cache.GetOrAdd(key, _ => new Lazy<TEntity>(entity)).Value;
    }

    /// <summary>
    /// 释放缓存资源
    /// </summary>
    public void Dispose()
    {
        _cache.Clear();
    }
}
