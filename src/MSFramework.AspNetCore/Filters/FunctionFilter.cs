using MicroserviceFramework.AspNetCore.Extensions;
using MicroserviceFramework.Function;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace MicroserviceFramework.AspNetCore.Filters
{
	public class FunctionFilter : ActionFilterAttribute
	{
		public FunctionFilter()
		{
			Order = FilterOrders.FunctionFilter;
		}

		public override void OnActionExecuting(ActionExecutingContext context)
		{
			var provider = context.HttpContext.RequestServices;

			var functionPath = context.ActionDescriptor.GetActionPath();
			var repository = provider.GetService<IFunctionDefineRepository>();
			if (repository == null || !repository.IsAvailable())
			{
				throw new MicroserviceFrameworkException("Function 仓储不可用");
			}

			var function = repository.GetByCode(functionPath);
			if (function == null || !function.Enabled)
			{
				context.Result = new NotFoundResult();
			}
		}

		public override void OnActionExecuted(ActionExecutedContext context)
		{
		}
	}
}