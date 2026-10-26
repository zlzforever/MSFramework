using System;
using System.Threading;

namespace MicroserviceFramework.DistributedLocking;

/// <summary>
/// 分布式锁抽象。
/// <para>
/// 用于多实例部署下互斥执行全局动作（如数据库迁移、种子初始化）：
/// 未注册实现（单体部署）时调用方应直接执行动作、不加锁；
/// 注册了实现（如 MSFramework.Dapr 提供的 Dapr 分布式锁）时由实现保证跨实例互斥。
/// </para>
/// </summary>
public interface IDistributedLock
{
    /// <summary>
    /// 获取命名锁：阻塞直到获得或 <paramref name="cancellationToken"/> 取消（等待/重试策略由实现决定）。
    /// 返回的句柄在 <see cref="IDisposable.Dispose"/> 时释放锁；
    /// 持锁方应尽快完成临界区动作并释放（实现可配置过期时间，超时自动失效防死锁）。
    /// </summary>
    /// <param name="name">锁名称，全局唯一标识（如 "ef-migrate:OrderingContext"）</param>
    /// <param name="cancellationToken">取消令牌</param>
    /// <returns>锁句柄，Dispose 时释放锁</returns>
    IDisposable Acquire(string name, CancellationToken cancellationToken = default);
}
