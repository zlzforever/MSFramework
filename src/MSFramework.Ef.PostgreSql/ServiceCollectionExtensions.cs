using System;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Npgsql.EntityFrameworkCore.PostgreSQL.Infrastructure;

namespace MicroserviceFramework.Ef.PostgreSql
{
	public static class ServiceCollectionExtensions
	{
		public static EntityFrameworkBuilder AddNpgsql<TDbContext>(
			this EntityFrameworkBuilder builder, IConfiguration configuration,
			Action<NpgsqlDbContextOptionsBuilder> configure = null) where TDbContext : DbContextBase
		{
			builder.Services.AddNpgsql<TDbContext>(configuration, configure);
			return builder;
		}

		public static EntityFrameworkBuilder AddNpgsql<TDbContext1, TDbContext2>(
			this EntityFrameworkBuilder builder, IConfiguration configuration,
			Action<NpgsqlDbContextOptionsBuilder> configure = null) where TDbContext1 : DbContextBase
			where TDbContext2 : DbContextBase
		{
			builder.Services.AddNpgsql<TDbContext1>(configuration, configure);
			builder.Services.AddNpgsql<TDbContext2>(configuration, configure);
			return builder;
		}

		public static EntityFrameworkBuilder AddNpgsql<TDbContext1, TDbContext2, TDbContext3>(
			this EntityFrameworkBuilder builder, IConfiguration configuration,
			Action<NpgsqlDbContextOptionsBuilder> configure = null) where TDbContext1 : DbContextBase
			where TDbContext2 : DbContextBase
			where TDbContext3 : DbContextBase
		{
			builder.Services.AddNpgsql<TDbContext1>(configuration, configure);
			builder.Services.AddNpgsql<TDbContext2>(configuration, configure);
			builder.Services.AddNpgsql<TDbContext3>(configuration, configure);

			return builder;
		}

		public static IServiceCollection AddNpgsql<TDbContext>(
			this IServiceCollection services, IConfiguration configuration,
			Action<NpgsqlDbContextOptionsBuilder> configure = null) where TDbContext : DbContextBase
		{
			var action = new Action<DbContextOptionsBuilder>(x =>
			{
				var dbContextType = typeof(TDbContext);
				var entryAssemblyName = dbContextType.Assembly.GetName().Name;

				var optionDict = configuration.GetSection("DbContexts").Get<DbContextConfigurationCollection>();
				var option = optionDict.Get(dbContextType);

				x.UseNpgsql(option.ConnectionString, options =>
				{
					options.MigrationsHistoryTable(option.TablePrefix + "migrations_history");
					options.MaxBatchSize(option.MaxBatchSize);
					options.MigrationsAssembly(entryAssemblyName);
					options.UseQuerySplittingBehavior(QuerySplittingBehavior.SplitQuery);

					configure?.Invoke(options);
				});
			});

			services.AddDbContext<TDbContext>(action);

			return services;
		}
	}
}