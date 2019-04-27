using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using MSFramework.Domain;

namespace MSFramework.AspNetCore
{
	public static class ServiceCollectionExtensions
	{
		public static MSFrameworkBuilder UseAspNetCoreSession(this MSFrameworkBuilder builder)
		{
			builder.Services.AddSingleton<IHttpContextAccessor, HttpContextAccessor>();
			builder.Services.AddScoped<IMSFrameworkSession, MSFrameworkSession>();
			return builder;
		}
	}
}