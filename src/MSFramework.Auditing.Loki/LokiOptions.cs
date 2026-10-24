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
    ///     审计日志中单个字符串值的最大长度
    /// </summary>
    public int MaxValueLength { get; set; } = 2048;

    /// <summary>
    ///     需要脱敏的属性名片段；匹配时忽略大小写及分隔符
    /// </summary>
    public string[] SensitivePropertyNames { get; set; } =
    [
        "password", "passwd", "pwd", "token", "secret", "authorization", "cookie", "session",
        "connectionstring", "apikey", "accesskey", "privatekey"
    ];

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
