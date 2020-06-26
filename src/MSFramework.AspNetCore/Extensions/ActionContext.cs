using System.Linq;
using Microsoft.AspNetCore.Mvc;

namespace MSFramework.AspNetCore.Extensions
{
	public static class ActionExecutingContextExtensions
	{
		public static string GetClientIp(this ActionContext context)
		{
			var ip = context.HttpContext.Request.Headers["X-Forwarded-For"].FirstOrDefault();
			if (string.IsNullOrEmpty(ip))
			{
				ip = context.HttpContext.Connection.RemoteIpAddress?.ToString();
			}

			return ip;
		}
	}
}