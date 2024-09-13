using System;
using MicroserviceFramework.Domain;
using MicroserviceFramework.Utils;

namespace MicroserviceFramework.Ef.Repositories;

/// <summary>
///
/// </summary>
/// <param name="dbContextFactory"></param>
/// <typeparam name="TEntity"></typeparam>
/// <typeparam name="TKey"></typeparam>
public class ExternalEntityRepository<TEntity, TKey>(DbContextFactory dbContextFactory)
    : IExternalEntityRepository<TEntity, TKey>
    where TEntity : ExternalEntity<TKey>
    where TKey : IEquatable<TKey>
{
    /// <summary>
    ///
    /// </summary>
    /// <param name="factory"></param>
    /// <returns></returns>
    public TEntity GetOrCreate(Func<TEntity> factory)
    {
        Check.NotNull(factory, nameof(factory));
        var dbContext = dbContextFactory.GetDbContext<TEntity>();
        Check.NotNull(dbContext, nameof(dbContext));
        var item = factory();
        Check.NotNull(item, nameof(item));
        foreach (var entity in dbContext.Set<TEntity>().Local)
        {
            if (item.Equals(entity))
            {
                return entity;
            }
        }

        dbContext.Set<TEntity>().Local.Add(item);
        return item;
    }
}
