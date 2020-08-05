using System.Security.Claims;
using Microsoft.AspNetCore.Http;

namespace MSFramework.AspNetCore
{
	public class HttpContextSession : Application.ISession
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

		public HttpContext HttpContext => _accessor.HttpContext;
	}
}