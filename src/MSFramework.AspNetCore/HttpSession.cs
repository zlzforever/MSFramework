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
    private string _userId;
    private string _userName;
    private string _email;
    private string _phoneNumber;
    private HashSet<string> _roles;

    public HttpSession(IHttpContextAccessor accessor)
    {
        HttpContext = accessor?.HttpContext;
    }

    public string TraceIdentifier => HttpContext?.TraceIdentifier;

    public string UserId
    {
        get
        {
            if (_userId != null)
            {
                return _userId;
            }

            var userId = HttpContext?.User.GetValue(ClaimTypes.NameIdentifier, "sid", "sub");
            _userId = string.IsNullOrWhiteSpace(userId) ? string.Empty : userId;

            return _userId;
        }
    }

    public string UserName
    {
        get
        {
            if (_userName != null)
            {
                return _userName;
            }

            var userName = HttpContext?.User.GetValue(ClaimTypes.Name, "name");
            _userName = string.IsNullOrWhiteSpace(userName) ? string.Empty : userName;

            return _userName;
        }
    }

    public string Email
    {
        get
        {
            if (_email != null)
            {
                return _email;
            }

            var email = HttpContext?.User.GetValue(ClaimTypes.Email, "email");
            _email = string.IsNullOrWhiteSpace(email) ? string.Empty : email;

            return _email;
        }
    }

    public string PhoneNumber
    {
        get
        {
            if (_phoneNumber != null)
            {
                return _phoneNumber;
            }

            var phoneNumber = HttpContext?.User.GetValue(ClaimTypes.MobilePhone, "phone_number");
            _phoneNumber = string.IsNullOrWhiteSpace(phoneNumber) ? string.Empty : phoneNumber;

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

            if (HttpContext == null)
            {
                _roles = new HashSet<string>();
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
