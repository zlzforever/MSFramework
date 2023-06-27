using System;
using MicroserviceFramework.Domain;

namespace MicroserviceFramework.Ef.Repositories;

public interface IExternalEntityRepository
{
    TEntity GetOrCreate<TEntity, TKey>(Func<TEntity> factory) where TEntity : class, IEntity<TKey>;
}
