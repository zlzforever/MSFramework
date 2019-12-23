using Microsoft.Extensions.DependencyInjection;

namespace MSFramework.Ef
{
	public class EntityFrameworkBuilder
	{
		public EntityFrameworkBuilder(IServiceCollection services)
		{
			Services = services;
		}

		public IServiceCollection Services { get; }
	}
}