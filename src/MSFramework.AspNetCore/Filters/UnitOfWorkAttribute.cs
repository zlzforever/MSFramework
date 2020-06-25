using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.Extensions.DependencyInjection;
using MSFramework.Domain;

namespace MSFramework.AspNetCore.Filters
{
	[AttributeUsage(AttributeTargets.Class | AttributeTargets.Method)]
	public class UnitOfWork : ActionFilterAttribute
	{
		public UnitOfWork()
		{
			Order = FilterOrders.UnitOfWorkFilterOrder;
		}

		public override void OnResultExecuted(ResultExecutedContext context)
		{
			// FunctionFilter 必须在 uow 之后
			var uowManager =
				context.HttpContext.RequestServices.GetService<IUnitOfWorkManager>();
			uowManager?.Commit();
		}
	}
}