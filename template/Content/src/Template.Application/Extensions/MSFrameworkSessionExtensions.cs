using MSFramework.Domain;

namespace Template.Application.Extensions
{
	public static class MSFrameworkSessionExtensions
	{
		public static bool IsAdmin(this IMSFrameworkSession session)
		{
			var aspnetCoreSession = session as MSFramework.AspNetCore.MSFrameworkSession;
			if (aspnetCoreSession == null)
			{
				return false;
			}

			var httpContext = aspnetCoreSession.HttpContext;
			return httpContext.User.IsInRole("admin");
		}
	}
}