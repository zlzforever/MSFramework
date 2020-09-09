using System.Linq;
using System.Security.Claims;
using Microsoft.AspNetCore.Http;
using ISession = MicroserviceFramework.Application.ISession;

namespace MicroserviceFramework.AspNetCore
{
	public class HttpContextSession : ISession
	{
		private readonly IHttpContextAccessor _accessor;

		public HttpContextSession(IHttpContextAccessor accessor)
		{
			_accessor = accessor;
		}

		public string UserId
		{
			get
			{
				if (HttpContext?.User == null)
				{
					return null;
				}

				var value = HttpContext.User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
				if (string.IsNullOrWhiteSpace(value))
				{
					value = HttpContext.User.FindFirst("sid")?.Value;
				}

				if (string.IsNullOrWhiteSpace(value))
				{
					value = HttpContext.User.FindFirst("sub")?.Value;
				}

				return value;
			}
		}

		public string UserName
		{
			get
			{
				if (HttpContext?.User == null)
				{
					return null;
				}

				var value = HttpContext.User.FindFirst(ClaimTypes.Name)?.Value;
				if (string.IsNullOrWhiteSpace(value))
				{
					value = HttpContext.User.FindFirst("name")?.Value;
				}

				return value;
			}
		}

		public string Email
		{
			get
			{
				if (HttpContext?.User == null)
				{
					return null;
				}

				var value = HttpContext.User.FindFirst(ClaimTypes.Email)?.Value;
				if (string.IsNullOrWhiteSpace(value))
				{
					value = HttpContext.User.FindFirst("email")?.Value;
				}

				return value;
			}
		}

		public string PhoneNumber
		{
			get
			{
				if (HttpContext?.User == null)
				{
					return null;
				}

				var value = HttpContext.User.FindFirst(ClaimTypes.MobilePhone)?.Value;
				if (string.IsNullOrWhiteSpace(value))
				{
					value = HttpContext.User.FindFirst("phone_number")?.Value;
				}

				return value;
			}
		}

		public string[] Roles
		{
			get { return HttpContext?.User?.FindAll(ClaimTypes.Role).Select(x => x.Value).ToArray(); }
		}

		public HttpContext HttpContext => _accessor.HttpContext;
	}
}