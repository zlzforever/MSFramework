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
			TraceIdentifier = HttpContext.TraceIdentifier;

			if (accessor.HttpContext.User == null)
			{
				return;
			}

			UserId = HttpContext.User.GetValue(ClaimTypes.NameIdentifier, "sid", "sub");
			UserName = HttpContext.User.GetValue(ClaimTypes.Name, "name");
			Email = HttpContext.User.GetValue(ClaimTypes.Email, "email");
			PhoneNumber = HttpContext.User.GetValue(ClaimTypes.MobilePhone, "phone_number");

			var roles1 = HttpContext.User.FindAll(ClaimTypes.Role).Select(x => x.Value).ToList();
			var roles2 = HttpContext.User.FindAll("role").Select(x => x.Value).ToList();
			roles1.AddRange(roles2);
			Roles = roles1.Count == 0 ? EmptyRoles : new HashSet<string>(roles1);
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