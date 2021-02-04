using System.Threading.Tasks;
using MicroserviceFramework.AspNetCore.Extensions;
using MicroserviceFramework.FeatureManagement;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.Extensions.DependencyInjection;

namespace MicroserviceFramework.AspNetCore.Filters
{
	public class FeatureFilter : IAsyncActionFilter, IOrderedFilter
	{
		public async Task OnActionExecutionAsync(ActionExecutingContext context, ActionExecutionDelegate next)
		{
			var provider = context.HttpContext.RequestServices;

			var functionPath = context.ActionDescriptor.GetActionPath();
			var repository = provider.GetService<IFeatureRepository>();
			if (repository == null || !repository.IsAvailable())
			{
				throw new MicroserviceFrameworkException("Feature 仓储不可用");
			}

			var function = repository.GetByName(functionPath);
			if (function is not {Enabled: true})
			{
				context.Result = new NotFoundResult();
			}

			await next();
		}

		public int Order => Conts.FunctionFilter;
	}
}