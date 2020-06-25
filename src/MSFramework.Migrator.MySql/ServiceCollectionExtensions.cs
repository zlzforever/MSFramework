using System;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using MSFramework.Initializer;

namespace MSFramework.Migrator.MySql
{
	public static class ServiceCollectionExtensions
	{
		public static MSFrameworkBuilder UseMySqlMigrator(this MSFrameworkBuilder builder, Type type,
			string connectionString)
		{
			builder.Services.AddSingleton<InitializerBase>(provider => new MySqlMigrator(type, connectionString,
				provider.GetRequiredService<ILogger<MySqlMigrator>>()));
			return builder;
		}
	}
}