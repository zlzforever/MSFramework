using System;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.Extensions.Logging;

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
			context.HttpContext.Response.StatusCode = 500;
			_logger.LogError(context.Exception.ToString());
			context.Result = new ApiResult(GetInnerMessage(context.Exception), 10201);
		}

		private string GetInnerMessage(Exception ex)
		{
			return ex.InnerException != null ? GetInnerMessage(ex.InnerException) : ex.Message;
		}
	}
}