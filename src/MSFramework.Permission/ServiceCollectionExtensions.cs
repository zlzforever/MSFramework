using Microsoft.Extensions.DependencyInjection;
using MSFramework.Permission.Application;
using MSFramework.Permission.DomainService;

namespace MSFramework.Permission
{
	public static class ServiceCollectionExtensions
	{
		public static MSFrameworkBuilder AddPermission(this MSFrameworkBuilder builder)
		{
			builder.Services.AddSingleton<IPermissionHashService, PermissionHashService>();
			builder.Services.AddSingleton<IPermissionChecker, DefaultPermissionChecker>();
			return builder;
		}
	}
}