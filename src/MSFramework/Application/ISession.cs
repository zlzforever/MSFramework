using System.Collections.Generic;

namespace MicroserviceFramework.Application;

/// <summary>
///
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
    /// 用户名
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
    /// 用户的显示名称
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

    void Override(ISession session);
}
