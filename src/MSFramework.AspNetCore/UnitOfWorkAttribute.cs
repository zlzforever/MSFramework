using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.Extensions.DependencyInjection;
using MSFramework.Domain;

namespace MSFramework.AspNetCore
{
	[AttributeUsage(AttributeTargets.Class | AttributeTargets.Method)]
	public class UnitOfWork : Attribute, IAsyncResultFilter
	{
		public async Task OnResultExecutionAsync(ResultExecutingContext context, ResultExecutionDelegate next)
		{
			// FunctionFilter 必须在 uow 之后
			var uowManager =
				context.HttpContext.RequestServices.GetService<IUnitOfWorkManager>();
			if (uowManager != null)
			{
				await uowManager.CommitAsync();
			}

			await next();
		}

		public int Order => 30000;
	}
}