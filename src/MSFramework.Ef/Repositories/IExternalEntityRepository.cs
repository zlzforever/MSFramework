using System;
using MicroserviceFramework.Domain;

namespace MicroserviceFramework.Ef.Repositories;

/// <summary>
///
/// </summary>
/// <typeparam name="TEntity"></typeparam>
/// <typeparam name="TKey"></typeparam>
public interface IExternalEntityRepository<TEntity, TKey>
    where TEntity : ExternalEntity<TKey> where TKey : IEquatable<TKey>
{
    /// <summary>
    ///
    /// </summary>
    /// <param name="factory"></param>
    /// <returns></returns>
    TEntity GetOrCreate(Func<TEntity> factory);
}
