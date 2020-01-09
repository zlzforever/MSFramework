using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using MSFramework.Data;
using MSFramework.Domain;

namespace MSFramework.AspNetCore
{
	public class MSFrameworkApiControllerBase : ControllerBase, IActionFilter, IAsyncActionFilter,
		IDisposable
	{
		protected IMSFrameworkSession Session { get; }

		protected ILogger Logger { get; }

		protected MSFrameworkApiControllerBase(IMSFrameworkSession session, ILogger logger)
		{
			Session = session;
			Logger = logger;
		}

		protected IActionResult PagedResult<TEntity>(PagedQueryResult<TEntity> result)
		{
			return new PagedApiResult<TEntity>(result);
		}

		protected IActionResult PagedResult<TEntity, TDTO>(PagedQueryResult<TEntity> result)
		{
			var mapper = HttpContext.RequestServices.GetRequiredService<IMapper>();
			var output = new PagedQueryResult<TDTO>
			{
				Limit = result.Limit,
				Total = result.Total,
				Page = result.Page,
				Entities = mapper.Map<List<TDTO>>(result.Entities)
			};
			return PagedResult(output);
		}

		protected IActionResult Ok(dynamic value, string msg = "")
		{
			return new ApiResult(value, msg);
		}

		protected IActionResult Failed(string msg = "", int code = 20000)
		{
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
				var errors = ModelState.Where(x => x.Value.ValidationState == ModelValidationState.Invalid).Select(x =>
					new
					{
						name = x.Key,
						error = x.Value.Errors.FirstOrDefault()?.ErrorMessage
					});

				context.Result = new ApiResult(new
				{
					success = false,
					code = 20000,
					msg = "数据校验不通过",
					errors
				});
				return;
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