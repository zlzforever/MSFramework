using System;
using System.Linq.Expressions;
using System.Threading;
using System.Threading.Tasks;

namespace MicroserviceFramework.Domain;

/// <summary>
/// 仓储接口
/// </summary>
public interface IRepository;

/// <summary>
/// 无键仓储接口，面向复合主键（多属性作主键、无 Id 包装）的聚合根。
/// <para>
/// 适用于实现非泛型 <see cref="IAggregateRoot"/> 的聚合根——例如以多个标量属性直接作为主键
/// （如 <c>OrderId</c> + <c>ProductId</c>）的实体。此类实体无法使用
/// <see cref="IRepository{TAggregateRoot,TKey}"/>（因为没有单一的 TKey），
/// 通过本接口配合表达式谓词（<see cref="FindAsync(System.Linq.Expressions.Expression{Func{TAggregateRoot,bool}},System.Threading.CancellationToken)"/>）
/// 完成查询，与 ABP Framework 的无键仓储设计对齐。
/// </para>
/// </summary>
/// <typeparam name="TAggregateRoot">聚合根类型，需实现非泛型 <see cref="IAggregateRoot"/></typeparam>
public interface IRepository<TAggregateRoot> : IRepository
    where TAggregateRoot : IAggregateRoot
{
    /// <summary>
    /// 通过表达式谓词查找聚合根，未找到或已软删除返回 null
    /// </summary>
    /// <param name="predicate">查询谓词，用于定位聚合根（复合主键实体按成员等值比较）</param>
    /// <param name="cancellationToken">取消令牌</param>
    /// <returns>匹配的聚合根，未找到返回 null</returns>
    Task<TAggregateRoot> FindAsync(Expression<Func<TAggregateRoot, bool>> predicate,
        CancellationToken cancellationToken = default);

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
