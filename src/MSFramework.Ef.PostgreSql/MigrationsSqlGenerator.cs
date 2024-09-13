using System.Linq;
using System.Reflection;
using MicroserviceFramework.Domain;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Migrations;
using Microsoft.EntityFrameworkCore.Migrations.Operations;
using Microsoft.Extensions.Logging;
using Npgsql.EntityFrameworkCore.PostgreSQL.Infrastructure.Internal;
using Npgsql.EntityFrameworkCore.PostgreSQL.Migrations;

namespace MicroserviceFramework.Ef.PostgreSql;

/// <summary>
///
/// </summary>
/// <param name="dependencies"></param>
/// <param name="npgsqlSingletonOptions"></param>
public class MigrationsSqlGenerator(
    MigrationsSqlGeneratorDependencies dependencies,
#pragma warning disable EF1001
    INpgsqlSingletonOptions npgsqlSingletonOptions)
#pragma warning restore EF1001
    : NpgsqlMigrationsSqlGenerator(dependencies, npgsqlSingletonOptions)
{
    /// <summary>
    ///
    /// </summary>
    public static bool RemoveForeignKey;
    /// <summary>
    ///
    /// </summary>
    public static bool RemoveExternalEntity;

    /// <summary>
    ///
    /// </summary>
    /// <param name="operation"></param>
    /// <param name="model"></param>
    /// <param name="builder"></param>
    protected override void Generate(
        MigrationOperation operation,
        IModel model,
        MigrationCommandListBuilder builder)
    {
        if (RemoveExternalEntity)
        {
            string table = null;
            if (operation is ITableMigrationOperation tableMigrationOperation)
            {
                table = tableMigrationOperation.Table;
            }
            else
            {
                var tableProperty =
                    operation.GetType().GetProperty("Table", BindingFlags.Instance | BindingFlags.Public);
                if (tableProperty != null)
                {
                    table = tableProperty.GetValue(operation) as string;
                }
            }

            if (!string.IsNullOrEmpty(table))
            {
                var entity = Dependencies.CurrentContext.Context.Model.GetEntityTypes()
                    .FirstOrDefault(x => x.GetTableName() == table);
                if (entity != null && entity.ClrType.IsAssignableTo(typeof(IExternalEntity)))
                {
                    Dependencies.Logger.Logger.LogInformation("Skip create table for external entity: {Table}",
                        table);
                    return;
                }
            }
        }

        if (RemoveForeignKey && operation is CreateTableOperation createTableOperation)
        {
            Dependencies.Logger.Logger.LogInformation("Skip create foreign key for table: {Table}",
                createTableOperation.Name);
            createTableOperation.ForeignKeys.Clear();
        }

        base.Generate(operation, model, builder);
    }
}
