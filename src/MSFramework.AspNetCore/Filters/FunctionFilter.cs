using System.Threading.Tasks;
using MicroserviceFramework.AspNetCore.Extensions;
using MicroserviceFramework.Function;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.Extensions.DependencyInjection;

namespace MicroserviceFramework.AspNetCore.Filters
{
	public class FunctionFilter : IAsyncActionFilter, IOrderedFilter
	{
		public async Task OnActionExecutionAsync(ActionExecutingContext context, ActionExecutionDelegate next)
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

			await next();
		}

		public int Order => Conts.FunctionFilter;
	}
}