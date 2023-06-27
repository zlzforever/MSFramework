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

    IReadOnlyCollection<string> Roles { get; }
    IReadOnlyCollection<string> Subjects { get; }
}
