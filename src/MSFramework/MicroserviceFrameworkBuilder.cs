using Microsoft.Extensions.DependencyInjection;

namespace MicroserviceFramework
{
	public class MicroserviceFrameworkBuilder
	{
		public MicroserviceFrameworkBuilder(IServiceCollection services)
		{
			Services = services;
		}

		public IServiceCollection Services { get; }
	}
}