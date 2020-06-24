using System;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc.Infrastructure;
using Microsoft.Extensions.DependencyInjection;
using MSFramework.AspNetCore.Function;
using MSFramework.Function;
using ISession = MSFramework.Domain.ISession;

namespace MSFramework.AspNetCore
{
	public static class ServiceCollectionExtensions
	{
		public static MSFrameworkBuilder AddAspNetCoreFunction<TFunctionStore>(this MSFrameworkBuilder builder)
			where TFunctionStore : class, IFunctionStore
		{
			builder.Services.AddSingleton<IFunctionFinder, AspNetCoreFunctionFinder>();
			builder.Services.AddScoped<IFunctionStore, TFunctionStore>();
			return builder;
		}

		public static MSFrameworkBuilder AddAspNetCore(this MSFrameworkBuilder builder)
		{
			builder.Services.AddSingleton<IHttpContextAccessor, HttpContextAccessor>();
			builder.Services.AddSingleton<IActionResultTypeMapper, ActionResultTypeMapper>();
			builder.Services.AddScoped<ISession, AspNetCoreSession>();
			return builder;
		}

		public static IApplicationBuilder UseMSFramework(this IApplicationBuilder builder,
			Action<IMSFrameworkApplicationBuilder> configure = null)
		{
			builder.ApplicationServices.UseMSFramework(configure);
			return builder;
		}
	}
}