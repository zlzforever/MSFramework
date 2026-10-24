using System;
using System.Collections.Generic;
using System.Linq;
using MicroserviceFramework.Application;

namespace MicroserviceFramework.LocalEvent;

/// <summary>
/// 会话标量字段快照。
/// 本地事件管道为跨请求作用域消费，直接捕获 Scoped 的 <see cref="ISession"/> 实例会导致
/// 作用域实例被错误地长期持有（生命周期泄漏），因此入队时仅拷贝标量字段。
/// </summary>
internal sealed class SessionSnapshot : ISession
{
    private static readonly SessionField[] SessionFields =
    [
        SessionField.Orientation,
        SessionField.DeviceId,
        SessionField.DeviceModel,
        SessionField.IMEI,
        SessionField.Platform,
        SessionField.Latitude,
        SessionField.Longitude,
        SessionField.Altitude,
        SessionField.Screen,
        SessionField.Battery,
        SessionField.Signal,
        SessionField.OSVersion,
        SessionField.Accuracy,
        SessionField.Bearing,
        SessionField.LocationSource,
        SessionField.Emulator
    ];

    private readonly Dictionary<string, string> _fields = new(StringComparer.OrdinalIgnoreCase);

    /// <summary>
    /// 从指定会话拷贝标量字段构建快照；session 为 null 时构建空快照
    /// </summary>
    /// <param name="session">来源会话，可为 null</param>
    public SessionSnapshot(ISession session)
    {
        if (session == null)
        {
            return;
        }

        CopyFrom(session);
    }

    /// <summary>
    /// 当前请求的跟踪标识
    /// </summary>
    public string TraceIdentifier { get; private set; }

    /// <summary>
    /// 用户标识
    /// </summary>
    public string UserId { get; private set; }

    /// <summary>
    /// 用户名
    /// </summary>
    public string UserName { get; private set; }

    /// <summary>
    /// 用户邮箱
    /// </summary>
    public string Email { get; private set; }

    /// <summary>
    /// 用户电话
    /// </summary>
    public string PhoneNumber { get; private set; }

    /// <summary>
    /// 用户的显示名称
    /// </summary>
    public string UserDisplayName { get; private set; }

    /// <summary>
    /// 用户所具有的角色
    /// </summary>
    public IReadOnlyCollection<string> Roles { get; private set; } = [];

    /// <summary>
    /// 用户所具有的主体：ID + 角色
    /// </summary>
    public IReadOnlyCollection<string> Subjects { get; private set; } = [];

    /// <summary>
    /// 获取入队时复制的请求头字段
    /// </summary>
    /// <param name="field">请求头字段</param>
    /// <returns>恒为 null</returns>
    public string GetValue(SessionField field)
    {
        return field != null && _fields.TryGetValue(field.HeaderKey, out var value) ? value : null;
    }

    /// <summary>
    /// 用指定会话覆盖快照的标量字段（与 HttpSession.Load 同款字段拷贝），
    /// 避免快照被当作可变会话使用时抛出 NotSupportedException
    /// </summary>
    /// <param name="session">来源会话，不可为 null</param>
    /// <exception cref="ArgumentNullException">session 为 null 时抛出</exception>
    public void Load(ISession session)
    {
        ArgumentNullException.ThrowIfNull(session);

        CopyFrom(session);
    }

    private void CopyFrom(ISession session)
    {
        TraceIdentifier = session.TraceIdentifier;
        UserId = session.UserId;
        UserName = session.UserName;
        Email = session.Email;
        PhoneNumber = session.PhoneNumber;
        UserDisplayName = session.UserDisplayName;
        Roles = session.Roles?.ToArray() ?? [];
        Subjects = session.Subjects?.ToArray() ?? [];

        _fields.Clear();
        foreach (var field in SessionFields)
        {
            var value = session.GetValue(field);
            if (value != null)
            {
                _fields[field.HeaderKey] = value;
            }
        }
    }
}
