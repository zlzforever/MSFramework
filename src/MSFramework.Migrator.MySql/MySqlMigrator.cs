using System;
using System.Data.Common;
using System.Threading.Tasks;
using Dapper;
using Microsoft.Extensions.Logging;
using MSFramework.Shared;
using MySql.Data.MySqlClient;

namespace MSFramework.Migrator.MySql
{
	public class MySqlMigrator : Migrator
	{
		protected override Database DataSource => Database.MySql;

		protected override DbConnection CreateConnection()
		{
			return new MySqlConnection(ConnectionString);
		}

		protected override async Task PrepareDatabaseAsync()
		{
			var mySqlConnectionStringBuilder = new MySqlConnectionStringBuilder(ConnectionString);
			var database = mySqlConnectionStringBuilder.Database;
			mySqlConnectionStringBuilder.Database = "mysql";
			await using var conn = new MySqlConnection(mySqlConnectionStringBuilder.ToString());
			await conn.OpenAsync();
			await conn.ExecuteAsync($"CREATE DATABASE IF NOT EXISTS `{database}`;");
		}

		public MySqlMigrator(Type type, string connectionString, ILogger<MySqlMigrator> logger) : base(
			type, connectionString, logger)
		{
		}
	}
}