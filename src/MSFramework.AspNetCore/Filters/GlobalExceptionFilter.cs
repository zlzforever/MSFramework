using MicroserviceFramework.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.Extensions.Logging;

namespace MicroserviceFramework.AspNetCore.Filters
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
			_logger.LogError(context.Exception.ToString());

			if (context.Exception is MicroserviceFrameworkException e)
			{
				context.HttpContext.Response.StatusCode = 200;
				context.Result = new JsonResult(new Response(null, e.Message, false, e.Code));
			}
			else
			{
				context.HttpContext.Response.StatusCode = 400;
				context.Result = new JsonResult(new Response(null, "服务出小差", false, 1));
			}
		}
	}
}