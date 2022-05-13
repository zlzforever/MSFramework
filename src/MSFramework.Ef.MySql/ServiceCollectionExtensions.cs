using System;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

#if !NETSTANDARD2_0

#endif

namespace MicroserviceFramework.Ef.MySql
{
	public static class ServiceCollectionExtensions
	{
		public static EntityFrameworkBuilder AddMySql<TDbContext>(
			this EntityFrameworkBuilder builder, IConfiguration configuration,
			Action<MySqlDbContextOptionsBuilder> mySqlOptionsAction = null) where TDbContext : DbContextBase
		{
			builder.Services.AddMySql<TDbContext>(configuration, mySqlOptionsAction);
			return builder;
		}

		public static EntityFrameworkBuilder AddMySql<TDbContext1, TDbContext2>(
			this EntityFrameworkBuilder builder, IConfiguration configuration,
			Action<MySqlDbContextOptionsBuilder> mySqlOptionsAction = null) where TDbContext1 : DbContextBase
			where TDbContext2 : DbContextBase
		{
			builder.Services.AddMySql<TDbContext1>(configuration, mySqlOptionsAction);
			builder.Services.AddMySql<TDbContext2>(configuration, mySqlOptionsAction);
			return builder;
		}

		public static EntityFrameworkBuilder AddMySql<TDbContext1, TDbContext2, TDbContext3>(
			this EntityFrameworkBuilder builder, IConfiguration configuration,
			Action<MySqlDbContextOptionsBuilder> mySqlOptionsAction = null) where TDbContext1 : DbContextBase
			where TDbContext2 : DbContextBase
			where TDbContext3 : DbContextBase
		{
			builder.Services.AddMySql<TDbContext1>(configuration, mySqlOptionsAction);
			builder.Services.AddMySql<TDbContext2>(configuration, mySqlOptionsAction);
			builder.Services.AddMySql<TDbContext3>(configuration, mySqlOptionsAction);

			return builder;
		}

		public static IServiceCollection AddMySql<TDbContext>(
			this IServiceCollection services, IConfiguration configuration,
			Action<MySqlDbContextOptionsBuilder> mySqlOptionsAction = null) where TDbContext : DbContextBase
		{
			var action = new Action<DbContextOptionsBuilder>(x =>
			{
				var dbContextType = typeof(TDbContext);
				var optionDict = configuration.GetSection("DbContexts").Get<DbContextConfigurationCollection>();
				var option = optionDict.Get(dbContextType);

				var entryAssemblyName = !string.IsNullOrWhiteSpace(option.MigrationsAssembly)
					? option.MigrationsAssembly
					: dbContextType.Assembly.GetName().Name;

				x.UseMySql(option.ConnectionString, ServerVersion.AutoDetect(option.ConnectionString), options =>
				{
					// 配置优先，可以不重编译代码，修改配置响应变化
					mySqlOptionsAction?.Invoke(options);

					var migrationsHistoryTable = string.IsNullOrWhiteSpace(option.TablePrefix)
						? "__ef_migrations_history"
						: $"{option.TablePrefix}migrations_history";
					options.MigrationsHistoryTable(migrationsHistoryTable);
					options.MaxBatchSize(option.MaxBatchSize);
					options.MigrationsAssembly(entryAssemblyName);
				});
			});
			services.AddDbContext<TDbContext>(action);
			return services;
		}
	}
}