using MicroserviceFramework.Application;
using MicroserviceFramework.AspNetCore.Functions;
using MicroserviceFramework.AspNetCore.Infrastructure;
using MicroserviceFramework.Functions;
using MicroserviceFramework.Shared;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc.Infrastructure;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using ISession = MicroserviceFramework.Application.ISession;

namespace MicroserviceFramework.AspNetCore
{
	public static class ServiceCollectionExtensions
	{
		public static MicroserviceFrameworkBuilder UseAspNetCore(this MicroserviceFrameworkBuilder builder,
			bool enableFunction = true)
		{
			if (enableFunction)
			{
				builder.Services.TryAddSingleton<IFunctionFinder, AspNetCoreFunctionFinder>();
			}

			var httpContextAccessor = new HttpContextAccessor();
			ServiceLocator.SetLocator(type =>
			{
				return httpContextAccessor.HttpContext.RequestServices.GetService(type);
			});

			builder.Services.TryAddSingleton<IHttpContextAccessor>(httpContextAccessor);
			builder.Services.AddSingleton<IActionResultTypeMapper, ActionResultTypeMapper>();
			builder.Services.TryAddScoped<ISession, HttpContextSession>();
			return builder;
		}

		public static void UseMicroserviceFramework(this IApplicationBuilder builder)
		{
			builder.ApplicationServices.UseMicroserviceFramework();
		}

		public static IMvcBuilder ConfigureInvalidModelStateResponse(this IMvcBuilder builder)
		{
			builder.ConfigureApiBehaviorOptions(x =>
			{
				x.InvalidModelStateResponseFactory = InvalidModelStateResponseFactory.Instance;
			});
			return builder;
		}
	}
}