using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using MSFramework.Domain;

namespace MSFramework.AspNetCore
{
	public class MSFrameworkControllerBase : ControllerBase, IActionFilter, IAsyncActionFilter,
		IDisposable
	{
		protected IMSFrameworkSession Session { get; }

		protected ILogger Logger { get; }

		protected MSFrameworkControllerBase(IMSFrameworkSession session, ILogger logger)
		{
			Session = session;
			Logger = logger;
		}

		protected IActionResult Ok(dynamic value, string msg = "")
		{
			return new ApiResult(value, msg);
		}

		protected IActionResult Failed(string msg = "", int code = 20000)
		{
			if (code < 20000 && code >= 30000)
			{
				throw new MSFrameworkException("Failed code should be less than 30000 and greater than 20000");
			}

			return new ApiResult(new
			{
				success = false,
				code,
				msg
			})
			{
				StatusCode = 500
			};
		}

		[NonAction]
		public virtual async Task OnActionExecutionAsync(ActionExecutingContext context, ActionExecutionDelegate next)
		{
			if (context == null)
			{
				throw new ArgumentNullException(nameof(context));
			}

			if (next == null)
			{
				throw new ArgumentNullException(nameof(next));
			}

			if (!ModelState.IsValid)
			{
				context.Result = new ApiResult(new
				{
					success = false,
					code = 20000,
					msg = "数据校验失败"
				});
			}

			OnActionExecuting(context);

			if (context.Result != null)
			{
				return;
			}

			var nextContext = await next();
			
			OnActionExecuted(nextContext);
			
			var uowManager = context.HttpContext.RequestServices.GetService<IUnitOfWorkManager>();
			if (uowManager != null)
			{
				await uowManager.CommitAsync();
			}
		}


		[NonAction]
		public virtual void OnActionExecuting(ActionExecutingContext context)
		{
		}

		[NonAction]
		public virtual void OnActionExecuted(ActionExecutedContext context)
		{
		}

		/// <inheritdoc />
		public void Dispose()
		{
			Dispose(true);
		}

		/// <summary>
		/// Releases all resources currently used by this <see cref="T:Microsoft.AspNetCore.Mvc.Controller" /> instance.
		/// </summary>
		/// <param name="disposing"><c>true</c> if this method is being invoked by the <see cref="M:Microsoft.AspNetCore.Mvc.Controller.Dispose" /> method,
		/// otherwise <c>false</c>.</param>
		protected virtual void Dispose(bool disposing)
		{
		}
	}
}