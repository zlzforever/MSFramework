using System;
using MicroserviceFramework.Serialization;

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
    /// <param name="factory">实体工厂，仅在未缓存时调用</param>
    /// <returns>外部实体实例</returns>
    TEntity Load(Func<TEntity> factory);

    /// <summary>
    /// 加载或获取已缓存的实体。
    /// </summary>
    /// <param name="entity">外部实体</param>
    /// <returns>已缓存的实体实例</returns>
    TEntity Load(TEntity entity);
}
