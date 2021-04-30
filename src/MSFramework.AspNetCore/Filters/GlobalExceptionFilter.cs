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
			if (context.Exception is MicroserviceFrameworkFriendlyException e1)
			{
				context.HttpContext.Response.StatusCode = 200;
				context.Result = new ApiResult(string.Empty)
				{
					Success = false,
					Msg = e1.Message,
					Code = e1.Code
				};
			}
			else if (context.Exception.InnerException is MicroserviceFrameworkFriendlyException e2)
			{
				context.HttpContext.Response.StatusCode = 200;
				context.Result = new ApiResult(string.Empty)
				{
					Success = false,
					Msg = e2.Message,
					Code = e2.Code
				};
			}
			else
			{
				_logger.LogError(context.Exception.ToString());

				context.HttpContext.Response.StatusCode = 500;
				context.Result = new ApiResult(string.Empty)
				{
					Success = false,
					Msg = "系统内部错误",
					Code = 500
				};
			}
		}
	}
}