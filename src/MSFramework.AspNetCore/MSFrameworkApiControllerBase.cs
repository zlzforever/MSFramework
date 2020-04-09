using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.Extensions.Logging;
using MSFramework.Domain;
using MSFramework.Http;

namespace MSFramework.AspNetCore
{
	public class MSFrameworkApiControllerBase : ControllerBase, IAsyncResultFilter, IActionFilter, IAsyncActionFilter
	{
		protected IMSFrameworkSession Session { get; }

		protected ILogger Logger { get; }

		protected MSFrameworkApiControllerBase(IMSFrameworkSession session, ILogger logger)
		{
			Session = session;
			Logger = logger;
		}

		protected ApiResult ApiResult(dynamic value = null, string msg = "")
		{
			return new ApiResult(value, msg);
		}

		protected ErrorApiResult ErrorApiResult(string msg = "", int code = 20000)
		{
			HttpContext.Response.StatusCode = 400;
			return new ErrorApiResult(msg, code);
		}

		[NonAction]
		public virtual Task OnResultExecutionAsync(ResultExecutingContext context, ResultExecutionDelegate next)
		{
			if (context.Result is EmptyResult)
			{
				context.Result = new JsonResult(ApiResult());
			}

			return next();
		}

		[NonAction]
		public virtual void OnActionExecuting(ActionExecutingContext context)
		{
		}

		[NonAction]
		public virtual void OnActionExecuted(ActionExecutedContext context)
		{
		}

		[NonAction]
		public virtual async Task OnActionExecutionAsync(
			ActionExecutingContext context,
			ActionExecutionDelegate next)
		{
			if (context == null)
			{
				throw new ArgumentNullException(nameof(context));
			}

			if (next == null)
			{
				throw new ArgumentNullException(nameof(next));
			}

			OnActionExecuting(context);
			if (context.Result != null)
			{
				return;
			}

			OnActionExecuted(await next());
		}
	}
}