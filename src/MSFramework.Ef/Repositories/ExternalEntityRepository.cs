using System;
using MicroserviceFramework.Domain;
using MicroserviceFramework.Utils;

namespace MicroserviceFramework.Ef.Repositories;

public class ExternalEntityRepository<TEntity, TKey> : IExternalEntityRepository<TEntity, TKey>
    where TEntity : ExternalEntity<TKey> where TKey : IEquatable<TKey>
{
    private readonly DbContextFactory _dbContextFactory;

    public ExternalEntityRepository(DbContextFactory dbContextFactory)
    {
        _dbContextFactory = dbContextFactory;
    }

    public TEntity GetOrCreate(Func<TEntity> factory)
    {
        Check.NotNull(factory, nameof(factory));
        var dbContext = _dbContextFactory.GetDbContext<TEntity>();
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
