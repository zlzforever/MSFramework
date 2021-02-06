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
		private static readonly HashSet<string> EmptyRoles = new();

		public HttpSession(IHttpContextAccessor accessor)
		{
			if (accessor?.HttpContext == null)
			{
				return;
			}

			HttpContext = accessor.HttpContext;
			UserId = HttpContext.User.GetValue(ClaimTypes.NameIdentifier, "sid", "sub");
			UserName = HttpContext.User.GetValue(ClaimTypes.Name, "name");
			Email = HttpContext.User.GetValue(ClaimTypes.Email, "email");
			PhoneNumber = HttpContext.User.GetValue(ClaimTypes.MobilePhone, "phone_number");
			TraceIdentifier = HttpContext.TraceIdentifier;

			var roles = HttpContext?.User?.FindAll(ClaimTypes.Role).Select(x => x.Value).ToArray();
			Roles = roles == null ? EmptyRoles : new HashSet<string>(roles);
		}

		public string TraceIdentifier { get; }

		public string UserId { get; }

		public string UserName { get; }

		public string Email { get; }

		public string PhoneNumber { get; }

		public HashSet<string> Roles { get; }

		public HttpContext HttpContext { get; }
	}
}