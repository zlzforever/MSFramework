using System.Threading.Tasks;
using MicroserviceFramework.AspNetCore.Extensions;
using MicroserviceFramework.Extensions;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.Extensions.Logging;

namespace MicroserviceFramework.AspNetCore.Filters
{
	public class LogFilter : IAsyncActionFilter
	{
		private readonly ILogger<LogFilter> _logger;

		public LogFilter(ILogger<LogFilter> logger)
		{
			_logger = logger;
		}

		public async Task OnActionExecutionAsync(ActionExecutingContext context, ActionExecutionDelegate next)
		{
			var userAgent = context.HttpContext.Request.Headers.GetOrDefault("User-Agent");
			var ip = context.GetRemoteIpAddress();
			_logger.LogInformation(
				$"{ip} -- {context.HttpContext.Request.Method.ToUpper()} \"{context.HttpContext.Request.Path}{context.HttpContext.Request.QueryString}\" \"{userAgent}\"");
			await next();
		}
	}
}