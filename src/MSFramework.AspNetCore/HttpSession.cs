using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.Security.Claims;
using MicroserviceFramework.Application;
using MicroserviceFramework.IdentityModel;
using Microsoft.AspNetCore.Http;
using ISession = MicroserviceFramework.Application.ISession;

namespace MicroserviceFramework.AspNetCore;

/// <summary>
///     基于 HTTP 上下文的用户会话实现，从 Claims 和请求头中提取用户信息
/// </summary>
public class HttpSession : ISession
{
    private readonly IHttpContextAccessor _accessor;
    private Dictionary<SessionField, string> _fields;

    /// <summary>
    ///     从 HttpContext 中解析 Claims 创建用户会话
    /// </summary>
    /// <param name="accessor">HTTP 上下文访问器</param>
    /// <returns>用户会话实例</returns>
    public static HttpSession Create(IHttpContextAccessor accessor)
    {
        if (accessor?.HttpContext == null)
        {
            return new HttpSession { Roles = [], Subjects = [] };
        }

        var user = accessor.HttpContext.User;

        // 单次遍历所有 claims，构建 type→value 字典并同时收集角色
        var claimMap = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
        var roles = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

        foreach (var claim in user.Claims)
        {
            claimMap.TryAdd(claim.Type, claim.Value);

            if (claim.Type == ClaimTypes.Role ||
                JwtClaimTypes.Role.Equals(claim.Type, StringComparison.OrdinalIgnoreCase))
            {
                roles.Add(claim.Value);
            }
        }

        var userName = GetFirstClaim(ClaimTypes.Name, JwtClaimTypes.Name);
        var givenName = GetFirstClaim(ClaimTypes.GivenName, JwtClaimTypes.GivenName);
        var familyName = GetFirstClaim(ClaimTypes.Surname, JwtClaimTypes.FamilyName);

        // 中文环境下，姓在前，名在后
        var name = CultureInfo.CurrentCulture.Name.StartsWith("zh", StringComparison.OrdinalIgnoreCase)
            ? $"{familyName}{givenName}"
            : $"{givenName}{familyName}";
        name = string.IsNullOrEmpty(name) ? GetFirstClaim(JwtClaimTypes.PreferredUserName) : name;
        name = string.IsNullOrEmpty(name) ? userName : name;

        var traceId = Activity.Current == null
            ? accessor.HttpContext.TraceIdentifier
            : Activity.Current.TraceId.ToString();

        var session = new HttpSession(accessor)
        {
            TraceIdentifier = traceId,
            UserId = GetFirstClaim(ClaimTypes.NameIdentifier, JwtClaimTypes.Subject),
            UserName = userName,
            Email = GetFirstClaim(ClaimTypes.Email, JwtClaimTypes.Email),
            // phone_number 优先， 一般能先获取到
            PhoneNumber = GetFirstClaim(JwtClaimTypes.PhoneNumber, ClaimTypes.MobilePhone),
            Roles = roles,
            UserDisplayName = name
        };

        var subjects = new HashSet<string> { session.UserId };
        foreach (var role in session.Roles)
        {
            subjects.Add(role);
        }

        session.Subjects = subjects;
        return session;

        string GetFirstClaim(params string[] types)
        {
            foreach (var type in types)
            {
                if (claimMap.TryGetValue(type, out var value) && !string.IsNullOrEmpty(value))
                {
                    return value;
                }
            }

            return null;
        }
    }

    private HttpSession()
    {
    }

    private HttpSession(IHttpContextAccessor accessor)
    {
        _accessor = accessor;
    }

    /// <summary>
    /// 获取指定字段的值。首次访问时从 Header <c>z-{kebab-case}</c> 读取并缓存，
    /// 后续访问直接返回缓存值。返回 <c>null</c> 表示 Header 中不存在该字段。
    /// </summary>
    public string GetValue(SessionField field)
    {
        _fields ??= new Dictionary<SessionField, string>();

        if (!_fields.TryGetValue(field, out var value))
        {
            value = ReadHeaderValue(field);
            _fields[field] = value;
        }

        return value;
    }

    /// <summary>
    /// 从 <see cref="HttpContext.Request.Headers"/> 读取单个字段的原始值。
    /// 可被子类重写以从其他来源取值（如 Cookie、Claims）。
    /// </summary>
    /// <param name="field">要读取的字段</param>
    /// <returns>Header 原始字符串，或 null</returns>
    protected virtual string ReadHeaderValue(SessionField field)
    {
        if (_accessor?.HttpContext == null)
        {
            return null;
        }

        var value = _accessor.HttpContext.Request.Headers[field.HeaderKey].ToString();
        return value.Length > 0 ? value : null;
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
    public IReadOnlyCollection<string> Roles { get; private set; }

    /// <summary>
    /// 用户所具有的主体：ID + 角色
    /// 主要用于权限系统进行检测
    /// </summary>
    public IReadOnlyCollection<string> Subjects { get; private set; }

    /// <summary>
    /// 覆盖当前用户的信息
    /// </summary>
    /// <param name="session"></param>
    public void Load(ISession session)
    {
        TraceIdentifier = session.TraceIdentifier;
        UserId = session.UserId;
        UserName = session.UserName;
        Email = session.Email;
        PhoneNumber = session.PhoneNumber;
        UserDisplayName = session.UserDisplayName;
        Roles = session.Roles;
        Subjects = session.Subjects;
    }
}
