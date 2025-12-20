using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Security.Claims;
using System.Text;
using MicroserviceFramework.AspNetCore.IdentityModel;
using MicroserviceFramework.Security.Claims;
using Microsoft.AspNetCore.Http;
using ISession = MicroserviceFramework.Application.ISession;

namespace MicroserviceFramework.AspNetCore;

/// <summary>
///
/// </summary>
public class HttpSession : ISession
{
    private static readonly HashSet<string> ChineseCultures =
    [
        "zh",
        "zh-CN",
        "zh-HK",
        "zh-MO",
        "zh-CHS",
        "zh-SG",
        "zh-TW",
        "zh-CHT",
        "zh-Hant",
        "zh-Hans"
    ];

    /// <summary>
    ///
    /// </summary>
    /// <param name="accessor"></param>
    /// <returns></returns>
    public static HttpSession Create(IHttpContextAccessor accessor)
    {
        if (accessor?.HttpContext == null)
        {
            return new HttpSession { Roles = Array.Empty<string>(), Subjects = Array.Empty<string>() };
        }

        var userName = accessor.HttpContext.User.GetValue(ClaimTypes.Name, JwtClaimTypes.Name);
        var givenName = accessor.HttpContext.User.GetValue(ClaimTypes.GivenName, JwtClaimTypes.GivenName);
        var familyName = accessor.HttpContext.User.GetValue(ClaimTypes.Surname, JwtClaimTypes.FamilyName);
        // 中文环境下，姓在前，名在后
        var name = ChineseCultures.Contains(CultureInfo.CurrentCulture.Name)
            ? $"{familyName}{givenName}"
            : $"{givenName}{familyName}";
        name = string.IsNullOrEmpty(name)
            ? accessor.HttpContext.User.GetValue(JwtClaimTypes.PreferredUserName)
            : name;
        name = string.IsNullOrEmpty(name) ? userName : name;
        var userDisplayName = name;

        var traceId = accessor.HttpContext.TraceIdentifier;

        var session = new HttpSession
        {
            TraceIdentifier = traceId,
            UserId = accessor.HttpContext.User.GetValue(ClaimTypes.NameIdentifier, JwtClaimTypes.Subject),
            UserName = userName,
            Email = accessor.HttpContext.User.GetValue(ClaimTypes.Email, JwtClaimTypes.Email),
            // phone_number 优先， 一般能先获取到， 优化性能
            PhoneNumber = accessor.HttpContext.User.GetValue(JwtClaimTypes.PhoneNumber, ClaimTypes.MobilePhone),
            Roles = accessor.HttpContext.User
                .FindAll(claim => claim.Type == ClaimTypes.Role ||
                                  JwtClaimTypes.Role.Equals(claim.Type, StringComparison.OrdinalIgnoreCase))
                .Select(x => x.Value).ToHashSet(),
            UserDisplayName = userDisplayName
        };

        var subjects = new List<string>();
        if (!string.IsNullOrEmpty(session.UserId))
        {
            subjects.Add(session.UserId);
        }

        foreach (var role in session.Roles)
        {
            if (!string.IsNullOrEmpty(role))
            {
                subjects.Add(role);
            }
        }

        session.Subjects = subjects;
        return session;
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

    private static string GetHeaderValue(IHeaderDictionary dict, params string[] headers)
    {
        foreach (var header in headers)
        {
            if (dict.TryGetValue(header, out var value))
            {
                return value;
            }
        }

        return null;
    }
}
