using System;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

#if !NETSTANDARD2_0

#endif

namespace MicroserviceFramework.Ef.MySql
{
	public static class ServiceCollectionExtensions
	{
		public static EntityFrameworkBuilder AddMySql<TDbContext>(
			this EntityFrameworkBuilder builder, IConfiguration configuration) where TDbContext : DbContextBase
		{
			builder.Services.AddMySql<TDbContext>(configuration);
			return builder;
		}

		public static EntityFrameworkBuilder AddMySql<TDbContext1, TDbContext2>(
			this EntityFrameworkBuilder builder, IConfiguration configuration) where TDbContext1 : DbContextBase
			where TDbContext2 : DbContextBase
		{
			builder.Services.AddMySql<TDbContext1>(configuration);
			builder.Services.AddMySql<TDbContext2>(configuration);
			return builder;
		}

		public static EntityFrameworkBuilder AddMySql<TDbContext1, TDbContext2, TDbContext3>(
			this EntityFrameworkBuilder builder, IConfiguration configuration) where TDbContext1 : DbContextBase
			where TDbContext2 : DbContextBase
			where TDbContext3 : DbContextBase
		{
			builder.Services.AddMySql<TDbContext1>(configuration);
			builder.Services.AddMySql<TDbContext2>(configuration);
			builder.Services.AddMySql<TDbContext3>(configuration);

			return builder;
		}

		public static IServiceCollection AddMySql<TDbContext>(
			this IServiceCollection services, IConfiguration configuration) where TDbContext : DbContextBase
		{
			var action = new Action<DbContextOptionsBuilder>(x =>
			{
				var dbContextType = typeof(TDbContext);
				var optionDict = configuration.GetSection("DbContexts").Get<DbContextConfigurationCollection>();
				var option = optionDict.Get(dbContextType);

				var entryAssemblyName = dbContextType.Assembly.GetName().Name;
#if NETSTANDARD2_0
				x.UseMySql(option.ConnectionString, options =>
				{
					options.MigrationsHistoryTable(option.TablePrefix + "migrations_history");
					options.MaxBatchSize(option.MaxBatchSize);
					options.MigrationsAssembly(entryAssemblyName);
					options.CharSet(Pomelo.EntityFrameworkCore.MySql.Storage.CharSet.Utf8Mb4);
				});
#else
				x.UseMySql(option.ConnectionString, ServerVersion.AutoDetect(option.ConnectionString), options =>
				{
					options.MigrationsHistoryTable(option.TablePrefix + "migrations_history");
					options.MaxBatchSize(option.MaxBatchSize);
					options.MigrationsAssembly(entryAssemblyName);
				});
#endif
			});
			services.AddDbContext<TDbContext>(action);
			return services;
		}
	}
}