using System;
using MicroserviceFramework.Domain;
using MicroserviceFramework.Utils;

namespace MicroserviceFramework.Ef.Repositories;

public class ExternalEntityRepository : IExternalEntityRepository
{
    private readonly DbContextFactory _dbContextFactory;

    public ExternalEntityRepository(DbContextFactory dbContextFactory)
    {
        _dbContextFactory = dbContextFactory;
    }

    public TEntity GetOrCreate<TEntity, TKey>(Func<TEntity> factory) where TEntity : class, IEntity<TKey>
    {
        Check.NotNull(factory, nameof(factory));
        var dbContext = _dbContextFactory.GetDbContext<TEntity>();
        Check.NotNull(dbContext, nameof(dbContext));
        var item = factory();
        Check.NotNull(item, nameof(item));
        foreach (var entity in dbContext.Set<TEntity>().Local)
        {
            if (item.Id.Equals(entity.Id))
            {
                return entity;
            }
        }

        dbContext.Set<TEntity>().Local.Add(item);
        return item;
    }
}
