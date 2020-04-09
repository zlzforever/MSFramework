using System;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using MSFramework.Audit;
using MSFramework.Domain;
using MSFramework.Domain.Repository;
using MSFramework.Ef.Audit;

namespace MSFramework.Ef
{
	public static class ServiceCollectionExtensions
	{
		public static MSFrameworkBuilder AddEfAuditStore(this MSFrameworkBuilder builder)
		{
			builder.Services.AddScoped<IAuditStore, EfAuditStore>();
			return builder;
		}

		public static MSFrameworkBuilder AddEntityFramework(this MSFrameworkBuilder builder,
			Action<EntityFrameworkBuilder> configure)
		{
			var eBuilder = new EntityFrameworkBuilder(builder.Services);
			configure?.Invoke(eBuilder);

			builder.Services.AddEntityFramework();
			return builder;
		}

		public static IServiceCollection AddEntityFramework(this IServiceCollection services)
		{
			services.AddEntityFrameworkOptionDict();
			services.AddSingleton<IEntityConfigurationTypeFinder, EntityConfigurationTypeFinder>(provider =>
			{
				var finder =
					new EntityConfigurationTypeFinder(provider
						.GetRequiredService<ILogger<EntityConfigurationTypeFinder>>());
				finder.Initialize();
				return finder;
			});
			services.AddScoped<DbContextFactory>();
			services.AddScoped<IUnitOfWorkManager, UnitOfWorkManager>();
			services.AddScoped(typeof(IRepository<>), typeof(EfRepository<>));
			services.AddScoped(typeof(IRepository<,>), typeof(EfRepository<,>));
			services.AddScoped(typeof(EfRepository<>), typeof(EfRepository<>));
			services.AddScoped(typeof(EfRepository<,>), typeof(EfRepository<,>));
			return services;
		}

		internal static void AddEntityFrameworkOptionDict(this IServiceCollection services)
		{
			services.AddSingleton(provider =>
			{
				var configuration = provider.GetRequiredService<IConfiguration>();
				return EntityFrameworkOptionsStore.LoadFrom(configuration);
			});
		}
	}
}