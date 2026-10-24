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
    /// 每个处理器的最大投递次数（包含首次投递）。
    /// </summary>
    public int MaxDeliveryAttempts { get; set; } = 3;

    /// <summary>
    /// 处理失败后的重试间隔。
    /// </summary>
    public TimeSpan RetryDelay { get; set; } = TimeSpan.FromSeconds(1);

    /// <summary>
    /// 停机时等待已接受事件排空的最长时间。
    /// </summary>
    public TimeSpan ShutdownDrainTimeout { get; set; } = TimeSpan.FromSeconds(30);
}
