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

				var userId = HttpContext.User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
				if (string.IsNullOrWhiteSpace(userId))
				{
					userId = HttpContext.User.FindFirst("sid")?.Value;
				}

				if (string.IsNullOrWhiteSpace(userId))
				{
					userId = HttpContext.User.FindFirst("sub")?.Value;
				}

				return userId;
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

				var userName = HttpContext.User.FindFirst(ClaimTypes.Name)?.Value;
				if (string.IsNullOrWhiteSpace(userName))
				{
					userName = HttpContext.User.FindFirst("name")?.Value;
				}

				return userName;
			}
		}

		public string[] Roles
		{
			get
			{
				return HttpContext?.User?.FindAll(ClaimTypes.Role).Select(x => x.Value).ToArray();
			}
		}

		public HttpContext HttpContext => _accessor.HttpContext;
	}
}