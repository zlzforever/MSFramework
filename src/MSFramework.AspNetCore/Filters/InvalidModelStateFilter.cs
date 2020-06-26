using System.Linq;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace MSFramework.AspNetCore.Filters
{
	public class InvalidModelStateFilter : ActionFilterAttribute
	{
		private ILogger _logger;
		
		public InvalidModelStateFilter()
		{
			Order = FilterOrders.InvalidModelStateFilter;
		}

		public override void OnActionExecuting(ActionExecutingContext context)
		{
			_logger = context.HttpContext.RequestServices.GetRequiredService<ILogger<InvalidModelStateFilter>>();
			_logger.LogDebug("Executing invalid model state filter");

			if (!context.ModelState.IsValid)
			{
				var errors = context.ModelState.Where(x => x.Value.ValidationState == ModelValidationState.Invalid)
					.Select(x =>
						new
						{
							name = x.Key,
							message = x.Value.Errors.FirstOrDefault()?.ErrorMessage
						});

				context.Result = new JsonResult(new
				{
					success = false,
					code = 1,
					msg = "数据校验不通过",
					errors
				})
				{
					StatusCode = 200
				};
			}
		}
		
		public override void OnActionExecuted(ActionExecutedContext context)
		{
			_logger.LogDebug("Executed invalid model state filter");
		}
	}
}