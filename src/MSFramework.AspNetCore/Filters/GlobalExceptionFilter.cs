using MicroserviceFramework.AspNetCore.Mvc;
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
				context.Result = new ApiResult(string.Empty)
				{
					Success = false,
					Msg = e.Message,
					Code = e.Code
				};
			}
			else
			{
				context.HttpContext.Response.StatusCode = 500;
				context.Result = new ApiResult(string.Empty)
				{
					Success = false,
					Msg = "服务出小差",
					Code = 500
				};
			}
		}
	}
}