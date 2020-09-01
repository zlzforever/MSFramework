using System;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace MicroserviceFramework.Ef.PostgreSql
{
	public static class ServiceCollectionExtensions
	{
		public static EntityFrameworkBuilder AddNpgsql<TDbContext>(
			this EntityFrameworkBuilder builder, IConfiguration configuration) where TDbContext : DbContextBase
		{
			builder.Services.AddNpgsql<TDbContext>(configuration);
			return builder;
		}

		public static EntityFrameworkBuilder AddNpgsql<TDbContext1, TDbContext2>(
			this EntityFrameworkBuilder builder, IConfiguration configuration) where TDbContext1 : DbContextBase
			where TDbContext2 : DbContextBase
		{
			builder.Services.AddNpgsql<TDbContext1>(configuration);
			builder.Services.AddNpgsql<TDbContext2>(configuration);
			return builder;
		}

		public static EntityFrameworkBuilder AddNpgsql<TDbContext1, TDbContext2, TDbContext3>(
			this EntityFrameworkBuilder builder, IConfiguration configuration) where TDbContext1 : DbContextBase
			where TDbContext2 : DbContextBase
			where TDbContext3 : DbContextBase
		{
			builder.Services.AddNpgsql<TDbContext1>(configuration);
			builder.Services.AddNpgsql<TDbContext2>(configuration);
			builder.Services.AddNpgsql<TDbContext3>(configuration);

			return builder;
		}

		public static IServiceCollection AddNpgsql<TDbContext>(
			this IServiceCollection services, IConfiguration configuration) where TDbContext : DbContextBase
		{
			var action = new Action<DbContextOptionsBuilder>(x =>
			{
				var dbContextType = typeof(TDbContext);
				var entryAssemblyName = dbContextType.Assembly.GetName().Name;

				var optionsConfiguration = new EntityFrameworkOptionsConfiguration(configuration);
				var option = optionsConfiguration.Get(dbContextType);

				x.UseNpgsql(option.ConnectionString, options => { options.MigrationsAssembly(entryAssemblyName); });
			});

			services.AddDbContext<TDbContext>(action);

			return services;
		}
	}
}