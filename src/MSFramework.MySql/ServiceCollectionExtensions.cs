using System;
using Microsoft.Extensions.DependencyInjection;
using MSFramework.Data;

namespace MSFramework.MySql
{
	public static class ServiceCollectionExtensions
	{
		public static MSFrameworkBuilder AddMySqlMigrator(this MSFrameworkBuilder builder)
		{
			builder.Services.AddTransient<IDatabaseMigrator, MySqlDatabaseMigrator>();
			return builder;
		}

		public static IMSFrameworkApplicationBuilder UseMySqlMigrator(this IMSFrameworkApplicationBuilder builder,
			Type type, string connectionString)
		{
			(builder.ApplicationServices
				.GetRequiredService<IDatabaseMigrator>() as MySqlDatabaseMigrator)?.Migrate(type, connectionString);
			return builder;
		}
	}
}