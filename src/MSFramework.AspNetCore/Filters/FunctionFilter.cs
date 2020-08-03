using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using MSFramework.AspNetCore.Extensions;
using MSFramework.Functions;

namespace MSFramework.AspNetCore.Filters
{
	public class FunctionFilter : ActionFilterAttribute
	{
		private ILogger _logger;

		public FunctionFilter()
		{
			Order = FilterOrders.FunctionFilter;
		}

		public override void OnActionExecuting(ActionExecutingContext context)
		{
			_logger = context.HttpContext.RequestServices.GetRequiredService<ILogger<FunctionFilter>>();
			_logger.LogDebug("Executing function filter");

			var provider = context.HttpContext.RequestServices;

			var functionPath = context.ActionDescriptor.GetActionPath();
			var repository = provider.GetService<IFunctionRepository>();
			if (repository == null)
			{
				throw new MSFrameworkException("未注册任何 FunctionStore");
			}

			var function = repository.GetByCode(functionPath);
			if (function == null || !function.Enabled)
			{
				context.Result = new NotFoundResult();
			}
		}

		public override void OnActionExecuted(ActionExecutedContext context)
		{
			_logger.LogDebug("Executed function filter");
		}
	}
}