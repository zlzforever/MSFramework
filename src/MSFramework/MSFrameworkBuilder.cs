using Microsoft.Extensions.DependencyInjection;

namespace MSFramework
{
	public class MSFrameworkBuilder
	{
		public MSFrameworkBuilder(IServiceCollection services)
		{
			Services = services;
		}

		public IServiceCollection Services { get; }
	}
}