using System;
using System.Data;
using System.Data.Common;
using Dapper;
using Microsoft.Extensions.Logging;
using MSFramework.Data;
using MySql.Data.MySqlClient;

namespace MSFramework.MySql
{
	public class MySqlDatabaseMigrator : DatabaseMigrator
	{
		public MySqlDatabaseMigrator(ILogger<DatabaseMigrator> logger) : base(logger)
		{
		}

		protected override DataSource DataSource => DataSource.MySql;

		protected override DbConnection CreateConnection(string connectionString)
		{
			return new MySqlConnection(connectionString);
		}

		protected override void PrepareDatabase(string connectionString)
		{
			var mySqlConnectionStringBuilder = new MySqlConnectionStringBuilder(connectionString);
			var database = mySqlConnectionStringBuilder.Database;
			mySqlConnectionStringBuilder.Database = "mysql";
			using var conn = new MySqlConnection(mySqlConnectionStringBuilder.ToString());
			conn.Open();
			conn.Execute($"CREATE DATABASE IF NOT EXISTS {database};");
		}
	}
}