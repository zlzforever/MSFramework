using System.Diagnostics.CodeAnalysis;
using MicroserviceFramework.Extensions.Options;
using Serilog.Sinks.Grafana.Loki;

namespace MicroserviceFramework.Auditing.Loki;

/// <summary>
///     Loki 审计日志存储的配置选项，从 appsettings.json 的 LokiAuditing 节读取
/// </summary>
[AutoOptions(Section = "LokiAuditing")]
[SuppressMessage("ReSharper", "CollectionNeverUpdated.Global")]
[SuppressMessage("ReSharper", "UnusedAutoPropertyAccessor.Global")]
public class LokiOptions
{
    /// <summary>
    ///     Grafana Loki 服务的 URI 地址
    /// </summary>
    public string Uri { get; set; }

    /// <summary>
    ///     作为标签发送的附加属性名称列表
    /// </summary>
    public string[] PropertiesAsLabels { get; set; }

    /// <summary>
    ///     Loki 基本身份验证凭据
    /// </summary>
    public LokiCredentials Credentials { get; set; }
}
