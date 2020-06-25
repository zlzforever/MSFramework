using System;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Logging;
using MSFramework.Domain;

namespace MSFramework.Ef
{
	public static class ServiceCollectionExtensions
	{
		public static MSFrameworkBuilder UseEntityFramework(this MSFrameworkBuilder builder,
			Action<EntityFrameworkBuilder> configure)
		{
			var eBuilder = new EntityFrameworkBuilder(builder.Services);
			configure?.Invoke(eBuilder);
			builder.Services.UseEntityFramework();
			return builder;
		}

		public static IServiceCollection UseEntityFramework(this IServiceCollection services)
		{
			services.TryAddSingleton(provider =>
			{
				var configuration = provider.GetRequiredService<IConfiguration>();
				return EntityFrameworkOptionsStore.LoadFrom(configuration);
			});
			services.TryAddSingleton<IEntityConfigurationTypeFinder>(provider =>
			{
				var finder =
					new EntityConfigurationTypeFinder(provider
						.GetRequiredService<ILogger<EntityConfigurationTypeFinder>>());
				((IEntityConfigurationTypeFinder) finder).Initialize();
				return finder;
			});
			services.TryAddScoped<DbContextFactory>();
			services.TryAddScoped(typeof(IRepository<>), typeof(EfRepository<>));
			services.TryAddScoped(typeof(IRepository<,>), typeof(EfRepository<,>));
			return services;
		}
	}
}