using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Security.Claims;
using MicroserviceFramework.AspNetCore.IdentityModel;
using MicroserviceFramework.Security.Claims;
using Microsoft.AspNetCore.Http;
using ISession = MicroserviceFramework.Application.ISession;

namespace MicroserviceFramework.AspNetCore;

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

        var session = new HttpSession
        {
            TraceIdentifier = accessor.HttpContext.TraceIdentifier,
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

    public string TraceIdentifier { get; private set; }

    public string UserId { get; private set; }

    public string UserName { get; private set; }

    public string Email { get; private set; }

    public string PhoneNumber { get; private set; }

    public string UserDisplayName { get; private set; }

    public IReadOnlyCollection<string> Roles { get; private set; }

    public IReadOnlyCollection<string> Subjects { get; private set; }

    public void Override(ISession session)
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
