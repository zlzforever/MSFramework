using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Microsoft.EntityFrameworkCore.Update;
using Microting.EntityFrameworkCore.MySql.Infrastructure.Internal;
using Microting.EntityFrameworkCore.MySql.Migrations;

namespace MicroserviceFramework.Ef.MySql;

/// <summary>
/// MySQL 迁移 SQL 生成器兼容类型。
/// 外部实体由模型元数据排除，不再通过全局状态修改普通建表操作。
/// </summary>
/// <param name="dependencies">迁移 SQL 生成器依赖</param>
/// <param name="commandBatchPreparer">命令批处理准备器</param>
/// <param name="options">MySQL 选项配置</param>
public class MigrationsSqlGenerator(
    MigrationsSqlGeneratorDependencies dependencies,
    ICommandBatchPreparer commandBatchPreparer,
#pragma warning disable EF1001
    IMySqlOptions options)
#pragma warning restore EF1001
    : MySqlMigrationsSqlGenerator(dependencies, commandBatchPreparer, options)
{
}
