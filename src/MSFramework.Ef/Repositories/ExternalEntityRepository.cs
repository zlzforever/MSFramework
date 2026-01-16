using System;
using System.Collections.Concurrent;
using System.Linq;
using MicroserviceFramework.Domain;
using MicroserviceFramework.Utils;
using Microsoft.EntityFrameworkCore;

namespace MicroserviceFramework.Ef.Repositories;

/// <summary>
///
/// </summary>
public class ExternalEntityRepository<TEntity, TKey>
    : IExternalEntityRepository<TEntity, TKey>
    where TEntity : ExternalEntity<TKey>
    where TKey : IEquatable<TKey>
{
    private readonly ConcurrentDictionary<object, dynamic> _cache = new();

    /// <summary>
    ///
    /// </summary>
    protected IQueryable<TEntity> Store { get; }

    /// <summary>
    ///
    /// </summary>
    /// <param name="dbContextFactory"></param>
    public ExternalEntityRepository(DbContextFactory dbContextFactory)
    {
        var dbContext = dbContextFactory.GetDbContext<TEntity>();
        Store = dbContext.Set<TEntity>().AsNoTracking();
    }

    /// <summary>
    ///
    /// </summary>
    /// <param name="factory"></param>
    /// <returns></returns>
    public TEntity Load(Func<TEntity> factory)
    {
        Check.NotNull(factory, nameof(factory));
        var item = factory();
        return Load(item);
    }

    /// <summary>
    ///
    /// </summary>
    /// <param name="entity"></param>
    /// <returns></returns>
    /// <exception cref="NotImplementedException"></exception>
    public TEntity Load(TEntity entity)
    {
        Check.NotNull(entity, nameof(entity));
        var key = entity.Id;
        return _cache.GetOrAdd(key, _ => entity);
    }

    /// <summary>
    ///
    /// </summary>
    public void Dispose()
    {
        _cache.Clear();
    }
}
