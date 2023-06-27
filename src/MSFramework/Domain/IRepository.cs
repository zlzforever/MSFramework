using System;
using System.Threading.Tasks;

namespace MicroserviceFramework.Domain;

/// <summary>
/// 仓储接口
/// </summary>
public interface IRepository
{
}

/// <summary>
/// 仓储接口
/// </summary>
public interface IRepository<TAggregateRoot, in TKey> : IRepository
    where TAggregateRoot : IAggregateRoot<TKey> where TKey : IEquatable<TKey>
{
    /// <summary>
    /// 添加新聚合根
    /// </summary>
    /// <param name="entity">Inserted entity</param>
    void Add(TAggregateRoot entity);

    /// <summary>
    /// Inserts a new entity.
    /// </summary>
    /// <param name="entity">Inserted entity</param>
    Task AddAsync(TAggregateRoot entity);

    /// <summary>
    /// Deletes an entity.
    /// </summary>
    /// <param name="entity">Entity to be deleted</param>
    void Delete(TAggregateRoot entity);

    /// <summary>
    /// Deletes an entity.
    /// </summary>
    /// <param name="entity">Entity to be deleted</param>
    Task DeleteAsync(TAggregateRoot entity);

    /// <summary>
    /// Gets an entity with given primary key.
    /// </summary>
    /// <param name="id">Primary key of the entity to get</param>
    /// <returns>Entity</returns>
    TAggregateRoot Find(TKey id);

    /// <summary>
    /// Gets an entity with given primary key.
    /// </summary>
    /// <param name="id">Primary key of the entity to get</param>
    /// <returns>Entity</returns>
    Task<TAggregateRoot> FindAsync(TKey id);

    /// <summary>
    /// Deletes an entity by primary key.
    /// </summary>
    /// <param name="id">Primary key of the entity</param>
    void Delete(TKey id);

    /// <summary>
    /// Deletes an entity by primary key.
    /// </summary>
    /// <param name="id">Primary key of the entity</param>
    Task DeleteAsync(TKey id);
}
