using System;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using MSFramework.Domain;

namespace MSFramework.AspNetCore.Filters
{
	[AttributeUsage(AttributeTargets.Class | AttributeTargets.Method)]
	public class UnitOfWork : ActionFilterAttribute
	{
		private ILogger _logger;

		public UnitOfWork()
		{
			Order = FilterOrders.UnitOfWork;
		}

		public override void OnActionExecuting(ActionExecutingContext context)
		{
			_logger = context.HttpContext.RequestServices.GetRequiredService<ILogger<InvalidModelStateFilter>>();
			_logger.LogDebug("Executing unit of work filter");
		}

		public override void OnActionExecuted(ActionExecutedContext context)
		{
			// FunctionFilter 必须在 uow 之后
			var uowManager =
				context.HttpContext.RequestServices.GetService<IUnitOfWorkManager>();
			uowManager?.Commit();

			_logger.LogDebug("Executed unit of work filter");
		}
	}
}