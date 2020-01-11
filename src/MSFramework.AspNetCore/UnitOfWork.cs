using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc.Filters;
using MSFramework.Domain;

namespace MSFramework.AspNetCore
{
	[AttributeUsage(AttributeTargets.Class | AttributeTargets.Method)]
	public class UnitOfWork : System.Attribute, IAsyncResultFilter, IOrderedFilter
	{
		public async Task OnResultExecutionAsync(ResultExecutingContext context, ResultExecutionDelegate next)
		{
			var uowManager =
				context.HttpContext.RequestServices.GetService(typeof(IUnitOfWorkManager));
			if (uowManager != null)
			{
				await ((IUnitOfWorkManager) uowManager).CommitAsync();
			}

			await next();
		}

		public int Order => int.MinValue;
	}
}