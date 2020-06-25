using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Http;
using ISession = MSFramework.Domain.ISession;

namespace MSFramework.AspNetCore
{
	public class AspNetCoreSession : ISession
	{
		private readonly IHttpContextAccessor _accessor;

		public AspNetCoreSession(IHttpContextAccessor accessor)
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