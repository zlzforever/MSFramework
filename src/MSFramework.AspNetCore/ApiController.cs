using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using MSFramework.Domain;

namespace MSFramework.AspNetCore
{
	public abstract class ApiController : ControllerBase, IAsyncResultFilter, IActionFilter, IAsyncActionFilter
	{
		protected ISession Session { get; private set; }

		protected ILogger Logger { get; private set; }
		
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

			Session = HttpContext.RequestServices.GetService<ISession>();
			Logger = HttpContext.RequestServices.GetRequiredService<ILoggerFactory>().CreateLogger(GetType());

			OnActionExecuting(context);

			if (context.Result != null)
			{
				return;
			}

			OnActionExecuted(await next());
		}
	}
}