using System.Linq;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Migrations;
using Microsoft.EntityFrameworkCore.Migrations.Operations;
using Microsoft.EntityFrameworkCore.Update;
using Pomelo.EntityFrameworkCore.MySql.Infrastructure.Internal;
using Pomelo.EntityFrameworkCore.MySql.Migrations;

namespace MicroserviceFramework.Ef.MySql;

public class RemoveForeignKeyMigrationsSqlGenerator : MySqlMigrationsSqlGenerator
{
    public RemoveForeignKeyMigrationsSqlGenerator(MigrationsSqlGeneratorDependencies dependencies,
        ICommandBatchPreparer commandBatchPreparer,
        IMySqlOptions options) : base(dependencies, commandBatchPreparer, options)
    {
    }

    protected override void Generate(
        CreateTableOperation operation,
        IModel model,
        MigrationCommandListBuilder builder,
        bool terminate = true)
    {
        operation.ForeignKeys
            .ToList()
            .ForEach(item => operation.ForeignKeys.Remove(item));
        base.Generate(operation, model, builder);
    }
}
