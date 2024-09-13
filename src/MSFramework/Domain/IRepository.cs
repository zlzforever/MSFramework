using System;
using System.Threading.Tasks;

namespace MicroserviceFramework.Domain;

/// <summary>
/// 仓储接口
/// </summary>
public interface IRepository;

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
    /// 添加新聚合根
    /// </summary>
    /// <param name="entity">Inserted entity</param>
    Task AddAsync(TAggregateRoot entity);

    /// <summary>
    /// 删除聚合根
    /// </summary>
    /// <param name="entity">Entity to be deleted</param>
    void Delete(TAggregateRoot entity);

    /// <summary>
    /// 删除聚合根
    /// </summary>
    /// <param name="entity">Entity to be deleted</param>
    Task DeleteAsync(TAggregateRoot entity);

    /// <summary>
    /// 通过主键查找聚合根
    /// </summary>
    /// <param name="id">Primary key of the entity to get</param>
    /// <returns>Entity</returns>
    TAggregateRoot Find(TKey id);

    /// <summary>
    /// 通过主键查找聚合根
    /// </summary>
    /// <param name="id">Primary key of the entity to get</param>
    /// <returns>Entity</returns>
    Task<TAggregateRoot> FindAsync(TKey id);

    /// <summary>
    /// 通过主键删除聚合根
    /// </summary>
    /// <param name="id">Primary key of the entity</param>
    void Delete(TKey id);

    /// <summary>
    /// 通过主键删除聚合根
    /// </summary>
    /// <param name="id">Primary key of the entity</param>
    Task DeleteAsync(TKey id);
}
