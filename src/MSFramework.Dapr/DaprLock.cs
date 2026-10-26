using System;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;
using Dapr;
using Dapr.Client;
using MicroserviceFramework.DistributedLocking;
using Microsoft.Extensions.Logging;

namespace MicroserviceFramework.Dapr;

/// <summary>
/// 基于 Dapr 分布式锁 API（POST /v1.0/lock、POST /v1.0/unlock）的锁实现。
/// 锁存储由 Dapr 组件提供（需支持 distributed lock API 的 state 组件，如 Redis）。
/// </summary>
/// <param name="daprClient">Dapr 客户端</param>
/// <param name="storeName">锁存储组件名称（Dapr 组件配置中的 name，默认 lockstore）</param>
/// <param name="acquireTimeout">单次获取锁的最大等待时长，超时抛出
/// <see cref="TimeoutException"/> 而非无限自旋等待（默认 120 秒；非正值回退默认）。
/// <see cref="IDistributedLock.Acquire"/> 为同步 API，无超时的重试自旋会无限期占用调用线程，
/// 在自动迁移等启动路径被调用时可能卡死整个进程启动</param>
/// <param name="expiry">锁过期时长（秒，默认 5 分钟）：持锁方崩溃后锁自动失效，避免死锁。
/// Dapr 侧锁到<b>期</b>自动失效但<b>句柄不感知</b>——临界区耗时超过过期时长时，
/// 其他实例可再次获锁并同时进入临界区（互斥性静默失效，仅在 Dispose 时告警），
/// 长临界区任务必须显式声明更长的过期时长或实现续租</param>
public class DaprLock(
    DaprClient daprClient,
    string storeName = "lockstore",
    TimeSpan acquireTimeout = default,
    TimeSpan expiry = default) : IDistributedLock
{
    /// <summary>
    /// 获取超时的默认时长：构造参数未指定（default）时使用。
    /// 取 120 秒是为了覆盖滚动发布时等待另一实例完成较慢迁移的场景；
    /// 超时后抛 <see cref="TimeoutException"/>，避免启动线程无限阻塞
    /// </summary>
    private static readonly TimeSpan DefaultAcquireTimeout = TimeSpan.FromSeconds(120);

    /// <summary>
    /// 锁过期时间的默认时长：构造参数未指定（default）时使用。
    /// 参考场景：EF 自动迁移的多实例互斥（正常迁移耗时远小于 5 分钟）
    /// </summary>
    private static readonly TimeSpan DefaultExpiry = TimeSpan.FromMinutes(5);

    /// <summary>
    /// 锁释放（Unlock）的最大等待时长：sidecar 不可达时 Dispose 不应阻塞到
    /// DaprClient 默认请求超时（约 100 秒）
    /// </summary>
    private static readonly TimeSpan UnlockTimeout = TimeSpan.FromSeconds(5);

    /// <summary>
    /// 获取失败后的重试间隔
    /// </summary>
    private static readonly TimeSpan RetryDelay = TimeSpan.FromMilliseconds(200);

    /// <summary>
    /// 重试间隔的随机抖动上限：多实例并发抢锁时固定间隔会同步碰撞（惊群），抖动错峰
    /// </summary>
    private static readonly int RetryDelayJitterMilliseconds = 200;

    /// <summary>
    /// 本实例的获取超时时长
    /// </summary>
    private readonly TimeSpan _acquireTimeout =
        acquireTimeout <= TimeSpan.Zero ? DefaultAcquireTimeout : acquireTimeout;

    /// <summary>
    /// 本实例的锁过期时长
    /// </summary>
    private readonly TimeSpan _expiry = expiry <= TimeSpan.Zero ? DefaultExpiry : expiry;

    /// <inheritdoc/>
    /// <exception cref="ArgumentNullException">name 为 null 时抛出</exception>
    /// <exception cref="TimeoutException">在 <c>acquireTimeout</c> 内未能获取到锁时抛出；
    /// 调用方可捕获后按业务语义降级（如跳过/排队/失败），而非无限等待</exception>
    /// <exception cref="OperationCanceledException">外部 cancellationToken 取消时抛出</exception>
    public IDisposable Acquire(string name, CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(name);

        // 预取消令牌：立即按契约抛 OCE——否则 while 条件直接为 false 跳过整个循环，
        // 末尾抛 TimeoutException，把"调用方主动取消"误报为"获取超时"
        cancellationToken.ThrowIfCancellationRequested();

        // 每次获取使用唯一 owner：仅持有者能释放自己的锁
        var lockOwner = Guid.NewGuid().ToString("N");

        // 链接外部取消与获取超时：任一触发即停止自旋，杜绝无限期阻塞
        using var timeoutCts = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
        timeoutCts.CancelAfter(_acquireTimeout);

        while (!timeoutCts.IsCancellationRequested)
        {
            try
            {
                var response = daprClient
                    .Lock(storeName, name, lockOwner, Math.Max(1, (int)_expiry.TotalSeconds), timeoutCts.Token)
                    .GetAwaiter()
                    .GetResult();
                if (response.Success)
                {
                    return new LockHandle(daprClient, storeName, name, lockOwner, _expiry);
                }
            }
            catch (OperationCanceledException) when (!cancellationToken.IsCancellationRequested)
            {
                // 契约语义（与 doc 一致）：外部令牌未取消时，取消只能来自内部超时令牌
                // （acquireTimeout 到期，或 Lock 调用中被超时切断）——转换为 TimeoutException，
                // 使调用方可按文档区分"获取超时"与"外部取消"
                throw new TimeoutException(
                    $"获取分布式锁 {name} 超时（等待 {_acquireTimeout.TotalSeconds:0} 秒）");
            }
            catch (DaprApiException ex) when (ex.IsTransient)
            {
                // sidecar/网络瞬时错误（Dapr 标记 IsTransient）：不立即让启动失败，
                // 在 acquireTimeout 内继续重试；非瞬时配置错误仍直接抛出，避免掩盖根因
                Defaults.Logger?.LogWarning(ex,
                    "获取 Dapr 分布式锁遇到瞬时错误，将重试: {LockName}", name);
            }

            // 以 Task.Delay 等待重试间隔而非 Thread.Sleep：等待期间超时/取消令牌触发时立即抛出，
            // 不再等满整个间隔；叠加随机抖动，避免多实例并发抢锁时同步自旋（惊群）
            var delay = RetryDelay +
                        TimeSpan.FromMilliseconds(Random.Shared.Next(0, RetryDelayJitterMilliseconds));

            try
            {
                Task.Delay(delay, timeoutCts.Token).GetAwaiter().GetResult();
            }
            catch (OperationCanceledException) when (!cancellationToken.IsCancellationRequested)
            {
                // 等待重试间隔期间 acquireTimeout 到期：同样按契约转换为 TimeoutException
                throw new TimeoutException(
                    $"获取分布式锁 {name} 超时（等待 {_acquireTimeout.TotalSeconds:0} 秒）");
            }
        }

        // 循环退出后的分流：外部令牌已取消时按契约抛 OCE（而非 TimeoutException），
        // 保证调用方可按文档区分"外部取消"与"获取超时"
        if (cancellationToken.IsCancellationRequested)
        {
            throw new OperationCanceledException("获取分布式锁被外部取消", cancellationToken);
        }

        throw new TimeoutException(
            $"获取分布式锁 {name} 超时（等待 {_acquireTimeout.TotalSeconds:0} 秒）");
    }

    /// <summary>
    /// 锁句柄：Dispose 时通过 Dapr Unlock 释放锁。
    /// 释放失败仅记录告警不抛出——锁会按过期时间自动失效，不影响临界区已完成的动作
    /// </summary>
    /// <param name="daprClient">Dapr 客户端</param>
    /// <param name="storeName">锁存储组件名称</param>
    /// <param name="name">锁名称</param>
    /// <param name="lockOwner">锁持有者标识（与获取时一致才能释放成功）</param>
    /// <param name="expiry">获取时的锁过期时长，用于 Dispose 时的互斥性哨兵检查</param>
    private sealed class LockHandle(
        DaprClient daprClient, string storeName, string name, string lockOwner, TimeSpan expiry)
        : IDisposable
    {
        /// <summary>
        /// 获锁时间戳（Stopwatch 时间基），用于临界区耗时与过期时长对比
        /// </summary>
        private readonly long _acquiredTimestamp = Stopwatch.GetTimestamp();

        /// <summary>
        /// Dispose 幂等哨兵：0 未释放、1 已释放
        /// </summary>
        private int _disposed;

        /// <summary>
        /// 释放锁：Unlock 失败仅记录告警（锁将按过期时间自动失效），不抛出异常——
        /// Dispose 抛异常会中断调用方的 using 块后续清理，代价高于"锁延迟失效"。
        /// 幂等：重复 Dispose 不再重复发起 Unlock（重复解锁因 owner 匹配规则会失败，
        /// 且会污染已由其他实例持有的同名锁状态）
        /// </summary>
        public void Dispose()
        {
            if (Interlocked.Exchange(ref _disposed, 1) != 0)
            {
                return;
            }

            try
            {
                // 限制释放耗时：sidecar 不可达时，无超时的 Unlock 会阻塞到
                // DaprClient 默认请求超时（约 100 秒），拖垮调用方清理路径
                using var cts = new CancellationTokenSource(UnlockTimeout);
                daprClient.Unlock(storeName, name, lockOwner, cts.Token)
                    .GetAwaiter()
                    .GetResult();
            }
            catch (Exception ex)
            {
                Defaults.Logger?.LogWarning(ex, "释放 Dapr 分布式锁失败（将按过期时间自动失效）: {LockName}", name);
            }
            finally
            {
                // 互斥性哨兵：临界区耗时超过过期时长时，Dapr 侧锁已自动失效，
                // 其他实例可能已持有该锁进入临界区。必须放在 finally：
                // Unlock 抛异常（例如锁已被其他实例抢走/owner 不匹配）时，
                // 这段最关键的告警不能被跳过。
                var elapsed = Stopwatch.GetElapsedTime(_acquiredTimestamp);
                if (elapsed >= expiry)
                {
                    Defaults.Logger?.LogWarning(
                        "分布式锁 {LockName} 的临界区耗时 {ElapsedSeconds:0.0} 秒已超过过期时长 " +
                        "{ExpirySeconds:0.0} 秒，锁已提前自动失效，互斥性可能已被破坏（其他实例可能同时进入临界区）",
                        name, elapsed.TotalSeconds, expiry.TotalSeconds);
                }
            }
        }
    }
}
