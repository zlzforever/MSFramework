using MicroserviceFramework.Domain;

namespace MicroserviceFramework.Auditing.Model;

/// <summary>
/// 审计操作来源，供 HTTP 请求与本地事件审计区分调用入口；依赖框架枚举基类，
/// 通过不可变共享实例供 EF 与 JSON 转换器解析。
/// </summary>
public sealed class AuditSource : Enumeration
{
    /// <summary>HTTP 请求来源，持久化及 JSON 标识为 Http。</summary>
    public static readonly AuditSource Http = new("Http");

    /// <summary>本地事件处理器来源，保留持久化及 JSON 标识 Local。</summary>
    public static readonly AuditSource LocalEvent = new("Local");

    private AuditSource(string id) : base(id, id)
    {
    }
}
