using System.Threading.Tasks;
using MicroserviceFramework.Application;
using MicroserviceFramework.AspNetCore.Extensions;
using MicroserviceFramework.Extensions;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.Extensions.Logging;

namespace MicroserviceFramework.AspNetCore.Filters
{
	public class LogFilter : IAsyncActionFilter
	{
		private readonly ILogger<LogFilter> _logger;
		private readonly ISession _session;

		public LogFilter(ILogger<LogFilter> logger, ISession session)
		{
			_logger = logger;
			_session = session;
		}

		public async Task OnActionExecutionAsync(ActionExecutingContext context, ActionExecutionDelegate next)
		{
			if (context.HasAttribute<IgnoreLogFilter>())
			{
				await next();
				return;
			}

			var userAgent = context.HttpContext.Request.Headers.GetOrDefault("User-Agent");
			var ip = context.GetRemoteIpAddress();
			_logger.LogInformation(
				$"{ip} -- {_session.UserId} -- {context.HttpContext.Request.Method.ToUpper()} \"{context.HttpContext.Request.Path}{context.HttpContext.Request.QueryString}\" \"{userAgent}\"");
			await next();
		}
	}
}