using System;
using System.Collections.Generic;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using MSFramework.Domain;

namespace MSFramework.AspNetCore.Filters
{
	[AttributeUsage(AttributeTargets.Class | AttributeTargets.Method)]
	public class UnitOfWorkFilter : IActionFilter, IOrderedFilter
	{
		private ILogger _logger;
		private static Dictionary<string, object> _methodDict;

		static UnitOfWorkFilter()
		{
			_methodDict = new Dictionary<string, object>
			{
				{"POST", null},
				{"DELETE", null},
				{"PATCH", null},
				{"PUT", null}
			};
		}

		public void OnActionExecuting(ActionExecutingContext context)
		{
			_logger = context.HttpContext.RequestServices.GetRequiredService<ILogger<InvalidModelStateFilter>>();
			_logger.LogDebug("Executing unit of work filter");
		}

		public void OnActionExecuted(ActionExecutedContext context)
		{
			// FunctionFilter 必须在 uow 之后
			if (_methodDict.ContainsKey(context.HttpContext.Request.Method))
			{
				var uowManager =
					context.HttpContext.RequestServices.GetService<IUnitOfWorkManager>();
				uowManager?.Commit();
				_logger.LogDebug("Executed unit of work filter");
			}
			else
			{
				_logger.LogDebug("Ignore unit of work filter");
			}
		}

		public int Order => FilterOrders.UnitOfWork;
	}
}