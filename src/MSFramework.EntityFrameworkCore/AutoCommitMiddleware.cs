using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;

namespace MSFramework.EntityFrameworkCore
{
	public class AutoCommitMiddleware
	{
		private readonly RequestDelegate _next;

		public AutoCommitMiddleware(RequestDelegate next)
		{
			_next = next;
		}

		public async Task Invoke(HttpContext context)
		{
			var dbContextFactory = context.RequestServices.GetRequiredService<DbContextFactory>();
			foreach (var dbContext in dbContextFactory.GetAllDbContexts())
			{
				await dbContext.CommitAsync();
			}

			await _next.Invoke(context);
		}
	}
}