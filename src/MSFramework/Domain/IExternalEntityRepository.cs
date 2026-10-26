using System;

namespace MicroserviceFramework.Domain;

/// <summary>
/// 外部实体仓储接口，用于加载不由本地 DbContext 管理的实体。
/// </summary>
/// <typeparam name="TEntity">外部实体类型</typeparam>
/// <typeparam name="TKey">主键类型</typeparam>
public interface IExternalEntityRepository<TEntity, TKey> : IDisposable
    where TEntity : ExternalEntity<TKey> where TKey : IEquatable<TKey>
{
    /// <summary>
    /// 使用工厂方法加载或获取已缓存的实体。
    /// </summary>
    /// <remarks>
    /// 缓存键来自实体自身（entity.Id），在工厂产物构造出来之前不可知，
    /// 因此本方法<b>会先执行一次工厂</b>；缓存命中时工厂产物被丢弃——
    /// 若工厂构造有成本或副作用，请改用 <see cref="Load(TKey,Func{TEntity})"/>
    /// 按主键惰性加载（命中缓存不执行工厂）
    /// </remarks>
    /// <param name="factory">实体工厂委托</param>
    /// <returns>缓存的实体（与工厂产物可能不是同一实例）</returns>
    TEntity Load(Func<TEntity> factory);

    /// <summary>
    /// 按主键惰性加载外部实体：缓存命中直接返回，未命中才执行工厂创建并缓存。
    /// </summary>
    /// <param name="key">外部实体主键（缓存键）</param>
    /// <param name="factory">实体工厂委托，仅在缓存未命中时调用</param>
    /// <returns>缓存的实体</returns>
    TEntity Load(TKey key, Func<TEntity> factory);

    /// <summary>
    /// 加载或获取已缓存的实体。
    /// </summary>
    /// <param name="entity">外部实体</param>
    /// <returns>已缓存的实体实例</returns>
    TEntity Load(TEntity entity);
}
