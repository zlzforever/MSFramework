using System;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.Extensions.Logging;
using MSFramework.Application;
using MSFramework.Http;

namespace MSFramework.AspNetCore
{
	public class HttpGlobalExceptionFilter : IExceptionFilter
	{
		private readonly ILogger<HttpGlobalExceptionFilter> _logger;

		public HttpGlobalExceptionFilter(ILogger<HttpGlobalExceptionFilter> logger)
		{
			_logger = logger;
		}

		public void OnException(ExceptionContext context)
		{
			context.HttpContext.Response.StatusCode = 400;
 
			_logger.LogError(context.Exception.ToString());

			if (context.Exception is MSFrameworkException e)
			{
				context.Result = new JsonResult(new ErrorApiResult(context.Exception.Message, e.Code));
			}
			// todo: only outerException print to UI
			else
			{
				context.Result = new JsonResult(new ErrorApiResult(context.Exception.Message));
			}
		}
	}
}