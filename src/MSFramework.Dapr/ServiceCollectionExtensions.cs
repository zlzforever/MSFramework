using System;
using Dapr.Client;
using MicroserviceFramework.DistributedLocking;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace MicroserviceFramework.Dapr;

/// <summary>
/// Dapr 分布式锁的依赖注入扩展方法
/// </summary>
public static class ServiceCollectionExtensions
{
    /// <summary>
    /// 使用 Dapr 分布式锁实现 <see cref="IDistributedLock"/>。
    /// <para>
    /// 需要 Dapr sidecar 与支持 distributed lock API 的 state 组件（如 Redis）。
    /// 注册后，多实例互斥动作（如 EF 自动迁移，见 EntityFrameworkInitializerBase）将持锁执行；
    /// 未注册时（单体部署）这些动作直接执行、不加锁。
    /// </para>
    /// </summary>
    /// <param name="builder">框架构建器</param>
    /// <param name="storeName">锁存储组件名称（Dapr 组件配置中 state 组件的 name，默认 lockstore）</param>
    /// <param name="expiry">锁过期时长（默认 5 分钟）。Dapr 侧锁到期自动失效但句柄不感知，
    /// 临界区可能超过默认值的长任务（如大 DDL 迁移）必须显式声明更长的过期时长</param>
    /// <param name="acquireTimeout">获取锁的最大等待时长（默认 120 秒）。
    /// 滚动发布时后启动实例需等待先启动实例完成迁移，通常应不小于最长迁移耗时</param>
    /// <returns>框架构建器</returns>
    /// <exception cref="ArgumentException">storeName 为 null 或空字符串时抛出</exception>
    public static MicroserviceFrameworkBuilder UseDaprLock(this MicroserviceFrameworkBuilder builder,
        string storeName = "lockstore", TimeSpan? expiry = null, TimeSpan? acquireTimeout = null)
    {
        ArgumentException.ThrowIfNullOrEmpty(storeName);

        // DaprClient（IDisposable，持 gRPC channel）注册为 DI 单例由容器统一释放：
        // 此前在工厂内 new 的实例不受容器释放管理，进程关闭时 channel 泄漏；
        // TryAdd 复用应用已注册的 DaprClient（如官方 AddDaprClient），避免双份连接
        builder.Services.TryAddSingleton(_ => new DaprClientBuilder().Build());
        builder.Services.TryAddSingleton<IDistributedLock>(sp =>
            new DaprLock(sp.GetRequiredService<DaprClient>(), storeName,
                acquireTimeout: acquireTimeout ?? default,
                expiry: expiry ?? default));
        return builder;
    }
}
