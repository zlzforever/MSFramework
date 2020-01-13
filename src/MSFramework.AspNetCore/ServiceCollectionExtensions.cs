using System;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using MSFramework.AspNetCore.Function;
using MSFramework.Domain;
using MSFramework.Function;

namespace MSFramework.AspNetCore
{
	public static class ServiceCollectionExtensions
	{
		public static MSFrameworkBuilder AddAspNetCoreFunction<FunctionStore>(this MSFrameworkBuilder builder)
			where FunctionStore : class, IFunctionStore
		{
			builder.Services.AddSingleton<IFunctionFinder, AspNetCoreFunctionFinder>();
			builder.Services.AddScoped<IFunctionStore, FunctionStore>();
			return builder;
		}

		public static MSFrameworkBuilder AddAspNetCoreSession(this MSFrameworkBuilder builder)
		{
			builder.Services.AddSingleton<IHttpContextAccessor, HttpContextAccessor>();
			builder.Services.AddScoped<IMSFrameworkSession, MSFrameworkSession>();
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