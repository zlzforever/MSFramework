using System;
using Microsoft.Extensions.DependencyInjection;
using MSFramework.Data;

namespace MSFramework.MySql
{
	public static class ServiceCollectionExtensions
	{
		public static MSFrameworkBuilder AddMySqlDatabaseMigration(this MSFrameworkBuilder builder)
		{
			builder.Services.AddTransient<IDatabaseMigration, MySqlDatabaseMigration>();
			return builder;
		}

		public static IMSFrameworkApplicationBuilder UseMySqlDatabaseMigration(this IMSFrameworkApplicationBuilder builder,
			Type type, string connectionString)
		{
			(builder.ApplicationServices
				.GetRequiredService<IDatabaseMigration>() as MySqlDatabaseMigration)?.Migrate(type, connectionString);
			return builder;
		}
	}
}