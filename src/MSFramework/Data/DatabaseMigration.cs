using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.IO;
using System.Linq;
using Dapper;
using Microsoft.Extensions.Logging;

namespace MSFramework.Data
{
	public abstract class DatabaseMigration : IDatabaseMigration
	{
		protected readonly ILogger Logger;
		protected readonly Type Type;
		protected readonly string ConnectionString;

		protected abstract DataSource DataSource { get; }

		protected virtual string MigrationsHistoryTable => "__migrations";

		protected virtual string MigrationsHistorySql => $@"
create table {MigrationsHistoryTable}
(
    migration_id    varchar(100) not null
        primary key,
    creation_time   datetime not null
)";

		protected virtual string InsertMigrationsHistorySql =>
			$"INSERT INTO {MigrationsHistoryTable} (migration_id, creation_time) VALUES (@MigrationId, @CreationTime);";

		protected abstract DbConnection CreateConnection(string connectionString);

		protected DatabaseMigration(Type type, string connectionString, ILogger<DatabaseMigration> logger)
		{
			Logger = logger;
			Type = type;
			ConnectionString = connectionString;
		}

		public void Execute()
		{
			var assembly = Type.Assembly;
			var pre = $"{assembly.GetName().Name}.DDL.";

			Logger.LogInformation($"Start migrate {DataSource} from assembly '{pre}'");

			var files = assembly.GetManifestResourceNames()
				.Where(x => x.StartsWith(pre)).ToList();

			var dict = new Dictionary<int, string>();
			foreach (var file in files)
			{
				var number = int.Parse(file.Replace(pre, "").Replace(".sql", ""));
				using var stream = assembly.GetManifestResourceStream(file);
				if (stream == null)
				{
					continue;
				}

				using var reader = new StreamReader(stream);
				dict.Add(number, reader.ReadToEnd());
			}

			var numbers = dict.Keys.ToList();
			numbers.Sort();
			Logger.LogInformation($"Find DDLs: {string.Join(",", numbers.Select(y => $"{y}.sql"))}");

			if (numbers.Count == 0)
			{
				return;
			}

			PrepareDatabase(ConnectionString);
			Logger.LogInformation($"Prepare database success");

			var conn = CreateConnection(ConnectionString);
			try
			{
				conn.Execute(MigrationsHistorySql);
			}
			catch (Exception e)
			{
				if (!e.Message.Contains("already exists"))
				{
					throw;
				}
			}

			var migrations = conn.Query<string>($"SELECT migration_id FROM {MigrationsHistoryTable}").ToList();
			if (conn.State == ConnectionState.Closed)
			{
				conn.Open();
			}

			var transaction = conn.BeginTransaction();
			foreach (var number in numbers)
			{
				if (!migrations.Contains(number.ToString()))
				{
					var sql = dict[number];
					if (!string.IsNullOrWhiteSpace(sql))
					{
						conn.Execute(sql, null, transaction);
					}

					Logger.LogInformation($"Execute DDL {number}.sql success");
					conn.Execute(InsertMigrationsHistorySql,
						new
						{
							MigrationId = number,
							CreationTime = DateTime.Now
						}, transaction);
				}
			}

			transaction.Commit();
		}

		protected abstract void PrepareDatabase(string connectionString);
	}
}