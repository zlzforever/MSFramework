using Microsoft.Extensions.DependencyInjection;

namespace MSFramework.AspNetCore.Permission
{
	public static class ServiceCollectionExtensions
	{
		public static MSFrameworkBuilder AddPermission(this MSFrameworkBuilder builder)
		{
			builder.Services.AddScoped<PermissionOptions>();
			builder.Services.AddScoped<ICerberusClient, CerberusClient>();
			builder.Services.AddScoped<AspNetCorePermissionFinder>();
			return builder;
		}
	}
}