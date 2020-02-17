using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using MSFramework.Data;

namespace MSFramework.AspNetCore.Permission
{
	public static class ServiceCollectionExtensions
	{
		public static MSFrameworkBuilder AddPermission(this MSFrameworkBuilder builder)
		{
			builder.Services.AddScoped<PermissionOptions>();
			builder.Services.AddScoped<CerberusClient>();
			builder.Services.AddScoped<AspNetCorePermissionFinder>();
			builder.Services.AddScoped<PermissionHandler>();
			return builder;
		}
	}
}