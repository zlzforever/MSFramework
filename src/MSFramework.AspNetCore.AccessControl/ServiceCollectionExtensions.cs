using Microsoft.Extensions.DependencyInjection.Extensions;

namespace MSFramework.AspNetCore.AccessControl
{
	public static class ServiceCollectionExtensions
	{
		public static MSFrameworkBuilder UseAccessControl(this MSFrameworkBuilder builder)
		{
			builder.Services.TryAddScoped<IAccessClient, AccessClient>();
			return builder;
		}
	}
}