using System;
using MicroserviceFramework.Ef.Infrastructure;
using MicroserviceFramework.Ef.Initializer;
using MicroserviceFramework.Initializer;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace MicroserviceFramework.Ef
{
	public static class ServiceCollectionExtensions
	{
		public static MicroserviceFrameworkBuilder UseEntityFramework(this MicroserviceFrameworkBuilder builder,
			Action<EntityFrameworkBuilder> configure)
		{
			var eBuilder = new EntityFrameworkBuilder(builder.Services);
			configure?.Invoke(eBuilder);
			builder.Services.UseEntityFramework();
			return builder;
		}

		public static IServiceCollection UseEntityFramework(this IServiceCollection services)
		{
			services.AddMemoryCache();
			services.TryAddSingleton<IEntityConfigurationTypeFinder>(_ =>
			{
				var finder =
					new EntityConfigurationTypeFinder();
				((IEntityConfigurationTypeFinder) finder).Initialize();
				return finder;
			});
			services.TryAddScoped<DbContextFactory>();
			services.AddInitializer<EntityFrameworkInitializer>();
			return services;
		}
	}
}