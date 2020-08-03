using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc.Infrastructure;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using MSFramework.AspNetCore.Functions;
using MSFramework.AspNetCore.Infrastructure;
using MSFramework.Functions;
using ISession = MSFramework.Application.ISession;

namespace MSFramework.AspNetCore
{
	public static class ServiceCollectionExtensions
	{
		public static MSFrameworkBuilder UseAspNetCore(this MSFrameworkBuilder builder)
		{
			builder.Services.TryAddSingleton<IFunctionFinder, AspNetCoreFunctionFinder>();
			builder.Services.TryAddSingleton<IHttpContextAccessor, HttpContextAccessor>();
			builder.Services.AddSingleton<IActionResultTypeMapper, ActionResultTypeMapper>();
			builder.Services.TryAddScoped<ISession, HttpContextSession>();
			return builder;
		}

		public static void UseMSFramework(this IApplicationBuilder builder)
		{
			builder.ApplicationServices.UseMSFramework();
		}

		public static IMvcBuilder UseInvalidModelStateResponse(this IMvcBuilder builder)
		{
			builder.ConfigureApiBehaviorOptions(x =>
			{
				x.InvalidModelStateResponseFactory = InvalidModelStateResponseFactory.Instance;
			});
			return builder;
		}
	}
}