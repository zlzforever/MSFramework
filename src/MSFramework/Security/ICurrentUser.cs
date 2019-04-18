using Microsoft.AspNetCore.Http;

namespace MSFramework.Security
{
	public interface ICurrentUser
	{
		string UserId { get; }
	}

	public class CurrentUser : ICurrentUser
	{
		private readonly IHttpContextAccessor _accessor;

		public CurrentUser(IHttpContextAccessor accessor)
		{
			_accessor = accessor;
		}

		public string UserId => _accessor.HttpContext.User.FindFirst("sub")?.Value;
	}
}