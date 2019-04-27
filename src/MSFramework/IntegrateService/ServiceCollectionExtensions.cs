using Microsoft.Extensions.DependencyInjection;

namespace MSFramework.IntegrateService
{
	public static class ServiceCollectionExtensions
	{
		public static MSFrameworkBuilder UseIntegrateService(this MSFrameworkBuilder builder)
		{
			builder.Services.AddScoped<IIntegrateService, IntegrateService>();
			return builder;
		}
	}
}