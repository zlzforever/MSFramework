using System;
using System.Diagnostics.CodeAnalysis;
using MicroserviceFramework.Extensions.Options;
using Serilog.Sinks.Grafana.Loki;

namespace MicroserviceFramework.Auditing.Loki;

/// <summary>
///     Loki 审计日志存储的配置选项，从 appsettings.json 的 LokiAuditing 节读取。
///     <b>本包已废弃</b>：审计存储请改用关系数据库（推荐 PostgreSQL）或自定义 IAuditingStore
/// </summary>
[AutoOptions(Section = "LokiAuditing")]
[Obsolete("MSFramework.Auditing.Loki 已废弃：审计存储请改用关系数据库（推荐 PostgreSQL）或自定义 IAuditingStore")]
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
