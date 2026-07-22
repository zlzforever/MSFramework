namespace DotNetCore.CAP.Dapr;

/// <summary>
/// CAP Dapr 集成选项配置
/// </summary>
public class DaprOptions
{
    /// <summary>
    /// 获取或设置 Dapr Pub/Sub 组件名称
    /// </summary>
    public string Pubsub { get; set; } = "pubsub";
}
