using System;

namespace MicroserviceFramework.Domain;

/// <summary>
///
/// </summary>
/// <typeparam name="TEntity"></typeparam>
/// <typeparam name="TKey"></typeparam>
public interface IExternalEntityRepository<TEntity, TKey> : IDisposable
    where TEntity : ExternalEntity<TKey> where TKey : IEquatable<TKey>
{
    /// <summary>
    ///
    /// </summary>
    /// <param name="factory"></param>
    /// <returns></returns>
    TEntity Load(Func<TEntity> factory);

    /// <summary>
    ///
    /// </summary>
    /// <param name="entity"></param>
    /// <returns></returns>
    TEntity Load(TEntity entity);
}
