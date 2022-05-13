using System;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace MicroserviceFramework.Ef.SqlServer
{
	public static class ServiceCollectionExtensions
	{
		public static EntityFrameworkBuilder AddSqlServer<TDbContext>(
			this EntityFrameworkBuilder builder, IConfiguration configuration,
			Action<SqlServerDbContextOptionsBuilder> sqlServerOptionsAction = null) where TDbContext : DbContextBase
		{
			builder.Services.AddDbContext<TDbContext>(x =>
			{
				var dbContextType = typeof(TDbContext);
				var optionDict = configuration.GetSection("DbContexts").Get<DbContextConfigurationCollection>();
				var option = optionDict.Get(dbContextType);
				var entryAssemblyName = !string.IsNullOrWhiteSpace(option.MigrationsAssembly)
					? option.MigrationsAssembly
					: dbContextType.Assembly.GetName().Name;
				x.UseSqlServer(option.ConnectionString, options =>
				{
					sqlServerOptionsAction?.Invoke(options);
					var migrationsHistoryTable = string.IsNullOrWhiteSpace(option.TablePrefix)
						? "__ef_migrations_history"
						: $"{option.TablePrefix}migrations_history";
					options.MigrationsHistoryTable(migrationsHistoryTable);
					options.MaxBatchSize(option.MaxBatchSize);
					options.MigrationsAssembly(entryAssemblyName);
				});
			});
			return builder;
		}
	}
}