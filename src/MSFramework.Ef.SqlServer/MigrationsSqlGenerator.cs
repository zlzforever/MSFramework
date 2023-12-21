using System.Linq;
using System.Reflection;
using MicroserviceFramework.Domain;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Migrations;
using Microsoft.EntityFrameworkCore.Migrations.Operations;
using Microsoft.EntityFrameworkCore.Update;
using Microsoft.Extensions.Logging;

namespace MicroserviceFramework.Ef.SqlServer;

public class MigrationsSqlGenerator(
    MigrationsSqlGeneratorDependencies dependencies,
    ICommandBatchPreparer commandBatchPreparer)
    : SqlServerMigrationsSqlGenerator(dependencies, commandBatchPreparer)
{
    public static bool RemoveForeignKey;
    public static bool RemoveExternalEntity;

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
