using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using MSFramework.AspNetCore.Infrastructure;
using MSFramework.Domain;

namespace MSFramework.AspNetCore
{
	public abstract class ApiControllerBase : ControllerBase, IAsyncResultFilter, IActionFilter, IAsyncActionFilter
	{
		protected ISession Session { get; private set; }

		protected ILogger Logger { get; private set; }

		protected ApiControllerBase()
		{
			Session = HttpContext.RequestServices.GetService<ISession>();
			Logger = HttpContext.RequestServices.GetRequiredService<ILoggerFactory>().CreateLogger(GetType());
		}

		protected IActionResult Error(string msg = "", int code = 20000)
		{
			HttpContext.Response.StatusCode = 400;
			return new JsonResult(new Response
			{
				Code = code,
				Msg = msg,
				Success = false
			});
		}

		[NonAction]
		public virtual Task OnResultExecutionAsync(ResultExecutingContext context, ResultExecutionDelegate next)
		{
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