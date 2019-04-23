using Microsoft.Extensions.DependencyInjection;

namespace MSFramework.EntityFrameworkCore
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