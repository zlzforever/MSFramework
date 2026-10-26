using System;

namespace MicroserviceFramework.LocalEvent;

/// <summary>
/// 本地事件配置选项
/// </summary>
public class LocalEventOptions
{
    /// <summary>
    /// 是否启用审计
    /// </summary>
    public bool EnableAuditing { get; set; }

    /// <summary>
    /// 发布端等待队列空位的最大时长，默认 30 秒；必须大于零且不超过 CancellationTokenSource 支持的范围。
    /// 超时抛出 TimeoutException，事件不会被本次等待成功入队；消费端仍保持尽力而为语义。
    /// </summary>
    public TimeSpan PublishTimeout { get; set; } = TimeSpan.FromSeconds(30);
}
