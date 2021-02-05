using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using MicroserviceFramework.Extensions;
using Microsoft.AspNetCore.Http;
using ISession = MicroserviceFramework.Application.ISession;

namespace MicroserviceFramework.AspNetCore
{
	public class HttpSession : ISession
	{
		private readonly IHttpContextAccessor _accessor;

		public HttpSession(IHttpContextAccessor accessor)
		{
			if (_accessor == null)
			{
				return;
			}

			_accessor = accessor;
			UserId = _accessor.HttpContext.User.GetValue(ClaimTypes.NameIdentifier, "sid", "sub");
			UserName = _accessor.HttpContext.User.GetValue(ClaimTypes.Name, "name");
			Email = _accessor.HttpContext.User.GetValue(ClaimTypes.Email, "email");
			PhoneNumber = _accessor.HttpContext.User.GetValue(ClaimTypes.MobilePhone, "phone_number");

			var roles = _accessor.HttpContext?.User?.FindAll(ClaimTypes.Role).Select(x => x.Value).ToArray();
			Roles = roles == null ? new HashSet<string>() : new HashSet<string>(roles);
		}

		public string TraceIdentifier => HttpContext.TraceIdentifier;

		public string UserId { get; }

		public string UserName { get; }

		public string Email { get; }

		public string PhoneNumber { get; }

		public HashSet<string> Roles { get; }

		public HttpContext HttpContext => _accessor.HttpContext;
	}
}