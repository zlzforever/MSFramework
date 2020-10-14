using Microsoft.AspNetCore.Mvc.Filters;

namespace MicroserviceFramework.AspNetCore.Filters
{
	public static class ServiceCollectionExtensions
	{
		public static FilterCollection AddUnitOfWork(this FilterCollection filters)
		{
			filters.Add<UnitOfWork>(FilterOrders.UnitOfWork);
			return filters;
		}

		public static FilterCollection AddFunctionFilter(this FilterCollection filters)
		{
			filters.Add<FunctionFilter>(FilterOrders.FunctionFilter);
			return filters;
		}

		public static FilterCollection AddAudit(this FilterCollection filters)

		{
			filters.Add<Audit>(FilterOrders.Audit);
			return filters;
		}

		public static FilterCollection AddGlobalException(this FilterCollection filters)

		{
			filters.Add<GlobalExceptionFilter>();
			return filters;
		}
	}
}