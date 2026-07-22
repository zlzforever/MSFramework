// using System;
// using System.Linq;
// using System.Reflection;
// using MicroserviceFramework.Domain;
// using Microsoft.EntityFrameworkCore;
// using Microsoft.EntityFrameworkCore.Metadata;
// using Microsoft.EntityFrameworkCore.Migrations;
// using Microsoft.EntityFrameworkCore.Migrations.Operations;
// using Microsoft.EntityFrameworkCore.Update;
// using Microsoft.Extensions.Logging;
// using Pomelo.EntityFrameworkCore.MySql.Infrastructure.Internal;
// using Pomelo.EntityFrameworkCore.MySql.Migrations;
//
// namespace MicroserviceFramework.Ef.MySql;
//
// /// <summary>
// /// MySQL 迁移 SQL 生成器，支持移除外键等自定义迁移行为
// /// </summary>
// /// <param name="dependencies">迁移 SQL 生成器依赖</param>
// /// <param name="commandBatchPreparer">命令批处理准备器</param>
// /// <param name="options">MySQL 选项配置</param>
// public class MigrationsSqlGenerator(
//     MigrationsSqlGeneratorDependencies dependencies,
//     ICommandBatchPreparer commandBatchPreparer,
// #pragma warning disable EF1001
//     IMySqlOptions options)
// #pragma warning restore EF1001
//     : MySqlMigrationsSqlGenerator(dependencies, commandBatchPreparer, options)
// {
//     /// <summary>
//     /// 是否在生成建表脚本时跳过外键约束
//     /// </summary>
//     public static bool RemoveForeignKey;
//
//
//     /// <summary>
//     /// 重写迁移操作生成逻辑，根据 RemoveForeignKey 配置跳过外键生成
//     /// </summary>
//     /// <param name="operation">迁移操作</param>
//     /// <param name="model">EF Core 模型</param>
//     /// <param name="builder">迁移命令列表构建器</param>
//     protected override void Generate(
//         MigrationOperation operation,
//         IModel model,
//         MigrationCommandListBuilder builder)
//     {
//         // comments by lewis 20250119: 在 Entity Configuration 中设置了 SetIsTableExcludedFromMigrations
//         // if (RemoveExternalEntity)
//         // {
//         //     string table = null;
//         //     if (operation is ITableMigrationOperation tableMigrationOperation)
//         //     {
//         //         table = tableMigrationOperation.Table;
//         //     }
//         //     else
//         //     {
//         //         var tableProperty =
//         //             operation.GetType().GetProperty("Table", BindingFlags.Instance | BindingFlags.Public);
//         //         if (tableProperty != null)
//         //         {
//         //             table = tableProperty.GetValue(operation) as string;
//         //         }
//         //     }
//         //
//         //     if (!string.IsNullOrEmpty(table))
//         //     {
//         //         var entity = Dependencies.CurrentContext.Context.Model.GetEntityTypes()
//         //             .FirstOrDefault(x => x.GetTableName() == table);
//         //         if (entity != null && entity.ClrType.IsAssignableTo(typeof(IExternalEntity)))
//         //         {
//         //             Dependencies.Logger.Logger.LogInformation("Skip create table for external entity: {Table}",
//         //                 table);
//         //             return;
//         //         }
//         //     }
//         // }
//         Console.WriteLine("Generate: {0}", operation.GetType().FullName);
//         if (RemoveForeignKey && operation is CreateTableOperation createTableOperation)
//         {
//             Dependencies.Logger.Logger.LogInformation("Skip create foreign key for table: {Table}",
//                 createTableOperation.Name);
//             Console.WriteLine("Skip create foreign key for table: {0}", createTableOperation.Name);
//             createTableOperation.ForeignKeys.Clear();
//         }
//
//         base.Generate(operation, model, builder);
//     }
// }
