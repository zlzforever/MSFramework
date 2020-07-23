using System;
using System.Collections.Generic;
using System.Data.Common;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Dapper;
using Microsoft.Extensions.Logging;
using MSFramework.Common;
using MSFramework.Initializer;

namespace MSFramework.Migrator
{
	public abstract class Migrator : InitializerBase, INotAutoRegisterInitializer
	{
		protected readonly ILogger Logger;
		protected readonly Type Type;

		public override int Order => 10;

		protected abstract Database DataSource { get; }

		protected virtual string MigrationHistoryTable => "__migration_history";

		protected abstract DbConnection CreateConnection();

		protected string ConnectionString { get; }

		protected virtual string CreateMigrationHistoryTableSql => $@"
create table {MigrationHistoryTable}
(
    migration_id    varchar(255) not null
        primary key,
    creation_time   datetime not null
)";

		protected virtual string AddMigrationHistorySql =>
			$"INSERT INTO {MigrationHistoryTable} (migration_id, creation_time) VALUES (@MigrationId, @CreationTime);";

		protected Migrator(Type type, string connectionString, ILogger logger)
		{
			Logger = logger;
			Type = type;
			ConnectionString = connectionString;
		}

		protected abstract Task PrepareDatabaseAsync();

		public override async Task InitializeAsync(IServiceProvider serviceProvider)
		{
			var assembly = Type.Assembly;
			var pre = $"{assembly.GetName().Name}.Migrations.";

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
				dict.Add(number, await reader.ReadToEndAsync());
			}

			var numbers = dict.Keys.ToList();
			numbers.Sort();
			Logger.LogInformation($"Find DDLs: {string.Join(",", numbers.Select(y => $"{y}.sql"))}");

			if (numbers.Count == 0)
			{
				return;
			}

			await PrepareDatabaseAsync();

			Logger.LogInformation($"Prepare database success");

			await CreateMigrationHistoryTable();

			var migrations = await GetMigrationsAsync();

			var conn = CreateConnection();
			foreach (var number in numbers)
			{
				if (!migrations.Contains(number.ToString()))
				{
					var sql = dict[number];
					if (!string.IsNullOrWhiteSpace(sql))
					{
						await conn.ExecuteAsync(sql);
					}

					Logger.LogInformation($"Execute {number}.sql success");
					await conn.ExecuteAsync(AddMigrationHistorySql,
						new
						{
							MigrationId = number,
							CreationTime = DateTime.Now
						});
				}
			}
		}

		protected virtual async Task<List<string>> GetMigrationsAsync()
		{
			var conn = CreateConnection();
			return (await conn.QueryAsync<string>($"SELECT migration_id FROM {MigrationHistoryTable}")).ToList();
		}

		protected virtual async Task CreateMigrationHistoryTable()
		{
			var conn = CreateConnection();
			try
			{
				await conn.ExecuteAsync(CreateMigrationHistoryTableSql);
			}
			catch (Exception e)
			{
				if (!e.Message.Contains("already exists"))
				{
					throw;
				}
			}
		}
	}
}