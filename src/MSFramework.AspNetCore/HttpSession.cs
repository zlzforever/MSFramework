using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using MicroserviceFramework.Security.Claims;
using Microsoft.AspNetCore.Http;
using ISession = MicroserviceFramework.Application.ISession;

namespace MicroserviceFramework.AspNetCore;

public class HttpSession : ISession
{
    internal static HttpSession Create(IHttpContextAccessor accessor)
    {
        if (accessor?.HttpContext == null)
        {
            return new HttpSession { Roles = Array.Empty<string>(), Subjects = new List<string>() };
        }

        var session = new HttpSession
        {
            TraceIdentifier = accessor.HttpContext.TraceIdentifier,
            UserId = accessor.HttpContext.User.GetValue(ClaimTypes.NameIdentifier, "sid", "sub"),
            UserName = accessor.HttpContext.User.GetValue(ClaimTypes.Name, "name"),
            Email = accessor.HttpContext.User.GetValue(ClaimTypes.Email, "email"),
            PhoneNumber = accessor.HttpContext.User.GetValue(ClaimTypes.MobilePhone, "phone_number"),
            Roles = accessor.HttpContext.User
                .FindAll(claim => claim.Type == ClaimTypes.Role ||
                                  "role".Equals(claim.Type, StringComparison.OrdinalIgnoreCase))
                .Select(x => x.Value).ToHashSet()
        };

        var subjects = new List<string>();
        if (!string.IsNullOrWhiteSpace(session.UserId))
        {
            subjects.Add(session.UserId);
        }

        foreach (var role in session.Roles)
        {
            if (!string.IsNullOrWhiteSpace(role))
            {
                subjects.Add(role);
            }
        }

        session.Subjects = subjects;
        return session;
    }

    public string TraceIdentifier { get; private init; }

    public string UserId { get; private init; }

    public string UserName { get; private init; }

    public string Email { get; private init; }

    public string PhoneNumber { get; private init; }

    public IReadOnlyCollection<string> Roles { get; private init; }

    public IReadOnlyCollection<string> Subjects { get; private set; }

    public HttpContext HttpContext { get; }
}
