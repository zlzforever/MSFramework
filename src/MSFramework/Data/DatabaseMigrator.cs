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
	public abstract class DatabaseMigrator : IDatabaseMigrator
	{
		private readonly ILogger _logger;

		protected abstract DataSource DataSource { get; }

		protected virtual string MigrationsHistoryTable => "__migrations_history";

		protected virtual string MigrationsHistorySql => $@"
create table if not exists {MigrationsHistoryTable}
(
    migration_id    varchar(100) not null
        primary key,
    creation_time   datetime not null
";

		protected virtual string InsertMigrationsHistorySql =>
			$"INSERT INTO __migrations_history (migration_id, creation_time) VALUES (@MigrationId, @CreationTime);";

		protected abstract DbConnection CreateConnection(string connectionString);

		protected DatabaseMigrator(ILogger<DatabaseMigrator> logger)
		{
			_logger = logger;
		}

		public void Migrate(Type type, string connectionString)
		{
			var assembly = type.Assembly;
			var pre = $"{assembly.GetName().Name}.DDL.";

			_logger.LogInformation($"Start migrate {DataSource} from assembly '{pre}'");

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
			_logger.LogInformation($"Find sql scripts: {string.Join(",", numbers.Select(y => $"{y}.sql"))}");

			var conn = PrepareDatabase(connectionString);
			_logger.LogInformation($"Prepare database success");

			conn.Execute(MigrationsHistorySql);
			var migrations = conn.Query<string>($"SELECT migration_id FROM __migrations_history").ToList();
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
					conn.Execute(sql, null, transaction);
					_logger.LogInformation($"Migrate {number}.sql success");
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

		protected abstract IDbConnection PrepareDatabase(string connectionString);
	}
}