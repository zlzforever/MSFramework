using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.Extensions.DependencyInjection;
using MSFramework.AspNetCore.Extensions;
using MSFramework.Function;

namespace MSFramework.AspNetCore.Filters
{
	public class FunctionFilter : ActionFilterAttribute
	{
		public FunctionFilter()
		{
			Order = FilterOrders.FunctionFilterOrder;
		}

		public override void OnActionExecuting(ActionExecutingContext context)
		{
			var provider = context.HttpContext.RequestServices;

			var functionPath = context.ActionDescriptor.GetActionPath();
			var functionStore = provider.GetService<IFunctionRepository>();
			if (functionStore == null)
			{
				throw new MSFrameworkException("未注册任何 FunctionStore");
			}

			var function = functionStore.GetByCode(functionPath);
			if (function == null || !function.Enabled)
			{
				context.Result = new NotFoundResult();
			}
		}
	}
}