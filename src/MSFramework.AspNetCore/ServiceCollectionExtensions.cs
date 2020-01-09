using System;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using MSFramework.Domain;

namespace MSFramework.AspNetCore
{
	public static class ServiceCollectionExtensions
	{
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