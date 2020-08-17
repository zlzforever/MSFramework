using System.Collections.Generic;
using System.Linq;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;
using Microsoft.EntityFrameworkCore.Migrations.Operations;
using Microsoft.EntityFrameworkCore.Storage;
using Pomelo.EntityFrameworkCore.MySql.Migrations.Internal;

namespace MSFramework.Ef.MySql
{
	public sealed class IgnoreForeignKeyMySqlMigrator : MySqlMigrator
	{
		public IgnoreForeignKeyMySqlMigrator(IMigrationsAssembly migrationsAssembly,
			IHistoryRepository historyRepository,
			IDatabaseCreator databaseCreator, IMigrationsSqlGenerator migrationsSqlGenerator,
			IRawSqlCommandBuilder rawSqlCommandBuilder, IMigrationCommandExecutor migrationCommandExecutor,
			IRelationalConnection connection, ISqlGenerationHelper sqlGenerationHelper,
			ICurrentDbContext currentContext, IDiagnosticsLogger<DbLoggerCategory.Migrations> logger,
			IDiagnosticsLogger<DbLoggerCategory.Database.Command> commandLogger, IDatabaseProvider databaseProvider) :
			base(migrationsAssembly, historyRepository, databaseCreator, migrationsSqlGenerator, rawSqlCommandBuilder,
				migrationCommandExecutor, connection, sqlGenerationHelper, currentContext, logger, commandLogger,
				databaseProvider)
		{
		}

		protected override IReadOnlyList<MigrationCommand> GenerateUpSql(
			Migration migration)
		{
			var createTableOperations = migration.UpOperations
				.OfType<CreateTableOperation>();
			foreach (var operation in createTableOperations)
			{
				operation.ForeignKeys.Clear();
			}

			return base.GenerateUpSql(migration);
		}
	}
}