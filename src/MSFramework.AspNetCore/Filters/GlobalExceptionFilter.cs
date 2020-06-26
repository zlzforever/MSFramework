using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.Extensions.Logging;
using MSFramework.AspNetCore.Api;

namespace MSFramework.AspNetCore.Filters
{
	public class GlobalExceptionFilter : IExceptionFilter
	{
		private readonly ILogger<GlobalExceptionFilter> _logger;

		public GlobalExceptionFilter(ILogger<GlobalExceptionFilter> logger)
		{
			_logger = logger;
		}

		public void OnException(ExceptionContext context)
		{
			context.HttpContext.Response.StatusCode = 400;

			_logger.LogError(context.Exception.ToString());

			if (context.Exception is MSFrameworkException e)
			{
				context.Result = new JsonResult(new Response(null, "服务出小差", false, e.Code));
			}
			// todo: only outerException print to UI
			else
			{
				context.Result = new JsonResult(new Response(null, "服务出小差", false, 1));
			}
		}
	}
}