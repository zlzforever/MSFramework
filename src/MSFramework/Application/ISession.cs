using System.Collections.Generic;

namespace MicroserviceFramework.Application;

/// <summary>
/// 当前请求会话信息，包含用户身份和设备上下文。
/// 通过 DI 注入，生命周期 Scoped。
/// </summary>
public interface ISession
{
    /// <summary>
    /// 当前请求的跟踪标识
    /// </summary>
    string TraceIdentifier { get; }

    /// <summary>
    /// 用户标识
    /// </summary>
    string UserId { get; }

    /// <summary>
    /// 登录用户名；HTTP 会话取 preferred_username，不使用姓名替代，缺失时为 null。
    /// </summary>
    string UserName { get; }

    /// <summary>
    /// 用户邮箱
    /// </summary>
    string Email { get; }

    /// <summary>
    /// 用户电话
    /// </summary>
    string PhoneNumber { get; }

    /// <summary>
    /// 用户显示名称；HTTP 会话依次取 name（兼容 ClaimTypes.Name）、按当前文化拼接的姓名、preferred_username。
    /// </summary>
    string UserDisplayName { get; }

    /// <summary>
    /// 用户所具有的角色
    /// </summary>
    IReadOnlyCollection<string> Roles { get; }

    /// <summary>
    /// 用户所具有的主体：ID + 角色
    /// 主要用于权限系统进行检测
    /// </summary>
    IReadOnlyCollection<string> Subjects { get; }

    /// <summary>
    /// 从请求头中提取指定字段的值。返回 <c>null</c> 表示 Header 中不存在该字段。
    /// </summary>
    /// <param name="field">要提取的字段</param>
    /// <returns>Header 原始值，或 null</returns>
    string GetValue(SessionField field);

    /// <summary>
    /// 覆盖当前用户的信息
    /// </summary>
    /// <param name="session"></param>
    void Load(ISession session);
}
