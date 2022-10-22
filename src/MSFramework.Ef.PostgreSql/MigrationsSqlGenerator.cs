// using Microsoft.EntityFrameworkCore.Infrastructure;
// using Microsoft.EntityFrameworkCore.Metadata;
// using Microsoft.EntityFrameworkCore.Migrations;
// using Microsoft.EntityFrameworkCore.Migrations.Operations;
// using Microsoft.Extensions.Options;
// using Npgsql.EntityFrameworkCore.PostgreSQL.Infrastructure.Internal;
// using Npgsql.EntityFrameworkCore.PostgreSQL.Migrations;
//
// namespace MicroserviceFramework.Ef.PostgreSql;
//
// public class MigrationsSqlGenerator : NpgsqlMigrationsSqlGenerator
// {
//     private string _tablePrefix;
//
//     public MigrationsSqlGenerator(MigrationsSqlGeneratorDependencies dependencies,
//         INpgsqlSingletonOptions npgsqlSingletonOptions) : base(
//         dependencies, npgsqlSingletonOptions)
//     {
//     }
//
//     private string TablePrefix
//     {
//         get
//         {
//             if (_tablePrefix == null)
//             {
//                 var type = Dependencies.CurrentContext.Context.GetType();
//                 var dbContextConfigurationCollection = Dependencies.CurrentContext.Context
//                     .GetService<IOptions<DbContextConfigurationCollection>>().Value;
//                 var configuration = dbContextConfigurationCollection.Get(type);
//                 _tablePrefix = string.IsNullOrWhiteSpace(configuration.TablePrefix)
//                     ? string.Empty
//                     : configuration.TablePrefix;
//             }
//
//             return _tablePrefix;
//         }
//     }
//
//     protected override void Generate(AddUniqueConstraintOperation operation, IModel model,
//         MigrationCommandListBuilder builder)
//     {
//         operation.Table = $"{TablePrefix}{operation.Table}";
//         operation.Name = $"{TablePrefix}{operation.Name}";
//
//         base.Generate(operation, model, builder);
//     }
//
//     protected override void ColumnDefinition(
//         AddColumnOperation operation,
//         IModel model,
//         MigrationCommandListBuilder builder)
//     {
//         ColumnDefinition(
//             operation.Schema,
//             operation.Table,
//             operation.Name,
//             operation,
//             model,
//             builder);
//     }
//
//     protected override void Generate(AddPrimaryKeyOperation operation, IModel model,
//         MigrationCommandListBuilder builder,
//         bool terminate = true)
//     {
//         operation.Table = $"{TablePrefix}{operation.Table}";
//         operation.Name = $"{TablePrefix}{operation.Name}";
//
//         base.Generate(operation, model, builder, terminate);
//     }
//
//     protected override void Generate(CreateTableOperation operation, IModel model, MigrationCommandListBuilder builder,
//         bool terminate = true)
//     {
//         operation.Name = $"{TablePrefix}{operation.Name}";
//
//         base.Generate(operation, model, builder, terminate);
//     }
//
//     protected override void Generate(AlterTableOperation operation, IModel model, MigrationCommandListBuilder builder)
//     {
//         operation.Name = $"{TablePrefix}{operation.Name}";
//
//         base.Generate(operation, model, builder);
//     }
//
//     protected override void Generate(RestartSequenceOperation operation, IModel model,
//         MigrationCommandListBuilder builder)
//     {
//         operation.Name = $"{TablePrefix}{operation.Name}";
//         base.Generate(operation, model, builder);
//     }
//
//     protected override void Generate(DropColumnOperation operation, IModel model, MigrationCommandListBuilder builder,
//         bool terminate = true)
//     {
//         operation.Table = $"{TablePrefix}{operation.Table}";
//
//         base.Generate(operation, model, builder, terminate);
//     }
//
//     protected override void Generate(AddColumnOperation operation, IModel model, MigrationCommandListBuilder builder,
//         bool terminate = true)
//     {
//         operation.Table = $"{TablePrefix}{operation.Table}";
//
//         base.Generate(operation, model, builder, terminate);
//     }
//
//     protected override void Generate(DropCheckConstraintOperation operation, IModel model,
//         MigrationCommandListBuilder builder)
//     {
//         operation.Table = $"{TablePrefix}{operation.Table}";
//         operation.Name = $"{TablePrefix}{operation.Name}";
//
//         base.Generate(operation, model, builder);
//     }
//
//     protected override void Generate(DropUniqueConstraintOperation operation, IModel model,
//         MigrationCommandListBuilder builder)
//     {
//         operation.Table = $"{TablePrefix}{operation.Table}";
//         operation.Name = $"{TablePrefix}{operation.Name}";
//
//         base.Generate(operation, model, builder);
//     }
//
//     protected override void Generate(AlterColumnOperation operation, IModel model, MigrationCommandListBuilder builder)
//     {
//         operation.Table = $"{TablePrefix}{operation.Table}";
//
//         base.Generate(operation, model, builder);
//     }
//
//     protected override void Generate(RenameIndexOperation operation, IModel model, MigrationCommandListBuilder builder)
//     {
//         operation.Table = $"{TablePrefix}{operation.Table}";
//         operation.Name = $"{TablePrefix}{operation.Name}";
//
//         base.Generate(operation, model, builder);
//     }
//
//     protected override void Generate(DropTableOperation operation, IModel model, MigrationCommandListBuilder builder,
//         bool terminate = true)
//     {
//         operation.Name = $"{TablePrefix}{operation.Name}";
//
//         base.Generate(operation, model, builder, terminate);
//     }
//
//     protected override void Generate(DropSequenceOperation operation, IModel model, MigrationCommandListBuilder builder)
//     {
//         operation.Name = $"{TablePrefix}{operation.Name}";
//
//         base.Generate(operation, model, builder);
//     }
//
//     protected override void Generate(DropPrimaryKeyOperation operation, IModel model,
//         MigrationCommandListBuilder builder,
//         bool terminate = true)
//     {
//         operation.Table = $"{TablePrefix}{operation.Table}";
//         operation.Name = $"{TablePrefix}{operation.Name}";
//
//         base.Generate(operation, model, builder, terminate);
//     }
//
//     protected override void PrimaryKeyConstraint(AddPrimaryKeyOperation operation, IModel model,
//         MigrationCommandListBuilder builder)
//     {
//         operation.Table = $"{TablePrefix}{operation.Table}";
//         operation.Name = $"{TablePrefix}{operation.Name}";
//
//         base.PrimaryKeyConstraint(operation, model, builder);
//     }
//
//     protected override void CheckConstraint(AddCheckConstraintOperation operation, IModel model,
//         MigrationCommandListBuilder builder)
//     {
//         operation.Table = $"{TablePrefix}{operation.Table}";
//         operation.Name = $"{TablePrefix}{operation.Name}";
//
//         base.CheckConstraint(operation, model, builder);
//     }
//
//     protected override void Generate(DropForeignKeyOperation operation, IModel model,
//         MigrationCommandListBuilder builder,
//         bool terminate = true)
//     {
//         operation.Table = $"{TablePrefix}{operation.Table}";
//         operation.Name = $"{TablePrefix}{operation.Name}";
//
//         base.Generate(operation, model, builder, terminate);
//     }
//
//     protected override void Generate(AddCheckConstraintOperation operation, IModel model,
//         MigrationCommandListBuilder builder)
//     {
//         operation.Table = $"{TablePrefix}{operation.Table}";
//         operation.Name = $"{TablePrefix}{operation.Name}";
//
//         base.Generate(operation, model, builder);
//     }
//
//     protected override void Generate(CreateIndexOperation operation, IModel model, MigrationCommandListBuilder builder,
//         bool terminate = true)
//     {
//         operation.Table = $"{TablePrefix}{operation.Table}";
//         operation.Name = $"{TablePrefix}{operation.Name}";
//
//         base.Generate(operation, model, builder, terminate);
//     }
//
//     protected override void Generate(AlterSequenceOperation operation, IModel model,
//         MigrationCommandListBuilder builder)
//     {
//         operation.Name = $"{TablePrefix}{operation.Name}";
//
//         base.Generate(operation, model, builder);
//     }
//
//     protected override void Generate(AddForeignKeyOperation operation, IModel model,
//         MigrationCommandListBuilder builder,
//         bool terminate = true)
//     {
//         operation.Table = $"{TablePrefix}{operation.Table}";
//         operation.Name = $"{TablePrefix}{operation.Name}";
//         operation.PrincipalTable = $"{TablePrefix}{operation.PrincipalTable}";
//
//         base.Generate(operation, model, builder, terminate);
//     }
//
//     protected override void Generate(CreateSequenceOperation operation, IModel model,
//         MigrationCommandListBuilder builder)
//     {
//         operation.Name = $"{TablePrefix}{operation.Name}";
//
//         base.Generate(operation, model, builder);
//     }
//
//     protected override void Generate(RenameSequenceOperation operation, IModel model,
//         MigrationCommandListBuilder builder)
//     {
//         operation.Name = $"{TablePrefix}{operation.Name}";
//
//         base.Generate(operation, model, builder);
//     }
//
//     protected override void Generate(RenameTableOperation operation, IModel model, MigrationCommandListBuilder builder)
//     {
//         operation.Name = $"{TablePrefix}{operation.Name}";
//
//         base.Generate(operation, model, builder);
//     }
//
//     protected override void Generate(DropIndexOperation operation, IModel model, MigrationCommandListBuilder builder,
//         bool terminate = true)
//     {
//         operation.Table = $"{TablePrefix}{operation.Table}";
//         operation.Name = $"{TablePrefix}{operation.Name}";
//
//         base.Generate(operation, model, builder, terminate);
//     }
//
//     protected override void Generate(RenameColumnOperation operation, IModel model, MigrationCommandListBuilder builder)
//     {
//         operation.Table = $"{TablePrefix}{operation.Table}";
//         operation.Name = $"{TablePrefix}{operation.Name}";
//
//         base.Generate(operation, model, builder);
//     }
//
//     protected override void Generate(UpdateDataOperation operation, IModel model, MigrationCommandListBuilder builder)
//     {
//         operation.Table = $"{TablePrefix}{operation.Table}";
//
//         base.Generate(operation, model, builder);
//     }
//
//     protected override void Generate(DeleteDataOperation operation, IModel model, MigrationCommandListBuilder builder)
//     {
//         operation.Table = $"{TablePrefix}{operation.Table}";
//
//         base.Generate(operation, model, builder);
//     }
//
//     protected override void Generate(InsertDataOperation operation, IModel model, MigrationCommandListBuilder builder,
//         bool terminate = true)
//     {
//         operation.Table = $"{TablePrefix}{operation.Table}";
//
//         base.Generate(operation, model, builder, terminate);
//     }
//
//
//     // public override IReadOnlyList<MigrationCommand> Generate(
//     //     IReadOnlyList<MigrationOperation> operations,
//     //     IModel model = null,
//     //     MigrationsSqlGenerationOptions options = MigrationsSqlGenerationOptions.Default)
//     // {
//     //     var type = Dependencies.CurrentContext.Context.GetType();
//     //     var dbContextConfigurationCollection = Dependencies.CurrentContext.Context
//     //         .GetService<IOptions<DbContextConfigurationCollection>>().Value;
//     //     var configuration = dbContextConfigurationCollection.Get(type);
//     //     foreach (var operation in operations)
//     //     {
//     //         switch (operation)
//     //         {
//     //             case CreateTableOperation createTableOperation:
//     //                 createTableOperation.Name = $"{configuration.TablePrefix}{createTableOperation.Name}";
//     //                 foreach (var columnOperation in createTableOperation.Columns)
//     //                 {
//     //                     columnOperation.Table = $"{configuration.TablePrefix}{columnOperation.Table}";
//     //                 }
//     //
//     //                 foreach (var checkConstraint in createTableOperation.CheckConstraints)
//     //                 {
//     //                     checkConstraint.Table = $"{configuration.TablePrefix}{checkConstraint.Table}";
//     //                 }
//     //
//     //                 foreach (var foreignKey in createTableOperation.ForeignKeys)
//     //                 {
//     //                     foreignKey.Table = $"{configuration.TablePrefix}{foreignKey.Table}";
//     //                     foreignKey.PrincipalTable = $"{configuration.TablePrefix}{foreignKey.PrincipalTable}";
//     //                     foreignKey.Name = $"{configuration.TablePrefix}{foreignKey.Name}";
//     //                 }
//     //
//     //                 if (createTableOperation.PrimaryKey != null)
//     //                 {
//     //                     createTableOperation.PrimaryKey.Name =
//     //                         $"{configuration.TablePrefix}{createTableOperation.PrimaryKey.Name}";
//     //                     createTableOperation.PrimaryKey.Table =
//     //                         $"{configuration.TablePrefix}{createTableOperation.PrimaryKey.Table}";
//     //                 }
//     //
//     //                 foreach (var uniqueConstraint in createTableOperation.UniqueConstraints)
//     //                 {
//     //                     uniqueConstraint.Table = $"{configuration.TablePrefix}{uniqueConstraint.Table}";
//     //                 }
//     //
//     //
//     //                 break;
//     //             case AlterTableOperation alterTableOperation:
//     //             {
//     //                 alterTableOperation.Name = $"{configuration.TablePrefix}{alterTableOperation.Name}";
//     //                 break;
//     //             }
//     //             case DropTableOperation dropTableOperation:
//     //             {
//     //                 dropTableOperation.Name = $"{configuration.TablePrefix}{dropTableOperation.Name}";
//     //                 break;
//     //             }
//     //             case CreateIndexOperation createIndexOperation:
//     //             {
//     //                 createIndexOperation.Name = $"{configuration.TablePrefix}{createIndexOperation.Name}";
//     //                 break;
//     //             }
//     //             case DropCheckConstraintOperation
//     //             case ITableMigrationOperation tableMigrationOperation:
//     //             {
//     //                 var tableProperty = operation.GetType().GetProperty("Table");
//     //                 var setter = tableProperty?.GetSetMethod();
//     //                 if (setter != null)
//     //                 {
//     //                     setter.Invoke(operation,
//     //                         new object[] { $"{configuration.TablePrefix}{tableMigrationOperation.Table}" });
//     //                 }
//     //
//     //                 break;
//     //             }
//     //             default:
//     //             {
//     //                 Console.WriteLine("Warning");
//     //                 break;
//     //             }
//     //         }
//     //     }
//     //
//     //     return base.Generate(operations, model, options);
//     // }
//
//     // protected override void Generate(
//     //     CreateTableOperation operation,
//     //     IModel model,
//     //     MigrationCommandListBuilder builder,
//     //     bool terminate = true)
//     // {
//     //
//     //     var type = Dependencies.CurrentContext.Context.GetType();
//     //     var options = Dependencies.CurrentContext.Context.GetService<IOptions<DbContextConfigurationCollection>>();
//     //     var configuration = options.Value.Get(type);
//     //     operation.Name = $"{configuration.TablePrefix}{operation.Name}";
//     //     base.Generate(operation, model, builder, terminate);
//     // }
// }
