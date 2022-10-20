using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using MicroserviceFramework.Security.Claims;
using Microsoft.AspNetCore.Http;
using ISession = MicroserviceFramework.Application.ISession;

namespace MicroserviceFramework.AspNetCore;

public class HttpSession : ISession
{
    private static readonly HashSet<string> EmptyRoles = new();
    private List<string> _subjects;
    private string _traceIdentifier;
    private string _userId;
    private string _userName;
    private string _email;
    private string _phoneNumber;
    private HashSet<string> _roles;

    public HttpSession(IHttpContextAccessor accessor)
    {
        if (accessor?.HttpContext == null)
        {
            return;
        }

        HttpContext = accessor.HttpContext;
    }

    public string TraceIdentifier
    {
        get
        {
            _traceIdentifier ??= HttpContext.TraceIdentifier;
            return _traceIdentifier;
        }
    }

    public string UserId
    {
        get
        {
            _userId ??= HttpContext.User.GetValue(ClaimTypes.NameIdentifier, "sid", "sub");
            return _userId;
        }
    }

    public string UserName
    {
        get
        {
            _userName ??= HttpContext.User.GetValue(ClaimTypes.Name, "name");
            return _userName;
        }
    }

    public string Email
    {
        get
        {
            _email ??= HttpContext.User.GetValue(ClaimTypes.Email, "email");
            return _email;
        }
    }

    public string PhoneNumber
    {
        get
        {
            _phoneNumber ??= HttpContext.User.GetValue(ClaimTypes.MobilePhone, "phone_number");
            return _phoneNumber;
        }
    }

    public IReadOnlyCollection<string> Roles
    {
        get
        {
            if (_roles != null)
            {
                return _roles;
            }

            var roles1 = HttpContext.User.FindAll(ClaimTypes.Role).Select(x => x.Value).ToList();
            var roles2 = HttpContext.User.FindAll("role").Select(x => x.Value).ToList();
            roles1.AddRange(roles2);
            _roles = roles1.Count == 0 ? EmptyRoles : new HashSet<string>(roles1);

            return _roles;
        }
    }

    public IReadOnlyCollection<string> Subjects
    {
        get
        {
            if (_subjects != null)
            {
                return _subjects;
            }

            _subjects = new List<string> { UserId };
            _subjects.AddRange(Roles);

            return _subjects;
        }
    }

    public HttpContext HttpContext { get; }
}
