using System.Linq;
using System.Reflection;
using MicroserviceFramework.Domain;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Migrations;
using Microsoft.EntityFrameworkCore.Migrations.Operations;
using Microsoft.EntityFrameworkCore.Update;
using Microsoft.Extensions.Logging;
using Pomelo.EntityFrameworkCore.MySql.Infrastructure.Internal;
using Pomelo.EntityFrameworkCore.MySql.Migrations;

namespace MicroserviceFramework.Ef.MySql;

/// <summary>
///
/// </summary>
/// <param name="dependencies"></param>
/// <param name="commandBatchPreparer"></param>
/// <param name="options"></param>
public class MigrationsSqlGenerator(
    MigrationsSqlGeneratorDependencies dependencies,
    ICommandBatchPreparer commandBatchPreparer,
#pragma warning disable EF1001
    IMySqlOptions options)
#pragma warning restore EF1001
    : MySqlMigrationsSqlGenerator(dependencies, commandBatchPreparer, options)
{
    /// <summary>
    /// 是否删除外键
    /// </summary>
    public static bool RemoveForeignKey;

    /// <summary>
    /// 是否删除外部实体
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
