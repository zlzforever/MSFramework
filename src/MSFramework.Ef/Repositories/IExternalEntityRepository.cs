using System;
using MicroserviceFramework.Domain;

namespace MicroserviceFramework.Ef.Repositories;

public interface IExternalEntityRepository<TEntity, TKey>
    where TEntity : ExternalEntity<TKey> where TKey : IEquatable<TKey>
{
    TEntity GetOrCreate(Func<TEntity> factory);
}
