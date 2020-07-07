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
		private readonly ILogger<UnitOfWorkFilter> _logger;
		private static readonly Dictionary<string, object> MethodDict;

		static UnitOfWorkFilter()
		{
			MethodDict = new Dictionary<string, object>
			{
				{"POST", null},
				{"DELETE", null},
				{"PATCH", null},
				{"PUT", null}
			};
		}

		public UnitOfWorkFilter(ILogger<UnitOfWorkFilter> logger)
		{
			_logger = logger;
		}

		public void OnActionExecuting(ActionExecutingContext context)
		{
			_logger.LogDebug("Executing unit of work filter");
		}

		public void OnActionExecuted(ActionExecutedContext context)
		{
			if (MethodDict.ContainsKey(context.HttpContext.Request.Method))
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