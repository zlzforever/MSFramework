using System.Linq;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace MSFramework.AspNetCore.Extensions
{
	public static class ActionExecutingContextExtensions
	{
		public static string GetRemoteIpAddress(this ActionContext context)
		{
			return context.HttpContext.GetRemoteIpAddress();
		}

		public static string GetRemoteIpAddress(this HttpContext context)
		{
			var ip = context.Request.Headers["X-Forwarded-For"].FirstOrDefault();
			if (string.IsNullOrEmpty(ip))
			{
				ip = context.Connection.RemoteIpAddress?.ToString();
			}

			return ip;
		}
	}
}