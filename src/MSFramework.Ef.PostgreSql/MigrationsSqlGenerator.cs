using System;
using System.Collections.Concurrent;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Migrations;
using Microsoft.EntityFrameworkCore.Migrations.Operations;
using Microsoft.Extensions.Logging;
using Npgsql.EntityFrameworkCore.PostgreSQL.Infrastructure.Internal;
using Npgsql.EntityFrameworkCore.PostgreSQL.Migrations;

namespace MicroserviceFramework.Ef.PostgreSql;

/// <summary>
/// PostgreSQL 迁移 SQL 生成器：按框架统一约定，不生成数据库外键（建表/新增/删除外键操作一律跳过），
/// 引用完整性由业务代码保证（配合软删除 ClientCascade 约定，禁止数据库级联删除）
/// </summary>
/// <param name="dependencies">迁移 SQL 生成器依赖</param>
/// <param name="npgsqlSingletonOptions">Npgsql 单例选项</param>
public class MigrationsSqlGenerator(
    MigrationsSqlGeneratorDependencies dependencies,
#pragma warning disable EF1001
    INpgsqlSingletonOptions npgsqlSingletonOptions)
#pragma warning restore EF1001
    : NpgsqlMigrationsSqlGenerator(dependencies, npgsqlSingletonOptions)
{
    /// <summary>
    /// 各 DbContext 的"迁移不生成外键"开关（按上下文类型隔离的进程级映射）：
    /// 由 <c>UseNoForeignKeyMigration</c> 以当前上下文类型为键写入，生成迁移 SQL 时按当前上下文读取。
    /// 相比旧实现的全局静态布尔，同一 provider 注册的多个 DbContext 不再互相泄漏开关状态
    /// </summary>
    private static readonly ConcurrentDictionary<Type, bool> RemoveForeignKeyByContextType = new();

    /// <summary>
    /// 为指定 DbContext 类型启用"迁移不生成外键"
    /// </summary>
    /// <param name="contextType">DbContext 类型（来自 DbContextOptions.ContextType）；
    /// 使用 EF 动态代理时与实例运行时类型可能不一致，代理场景请以注册类型为准</param>
    public static void SetRemoveForeignKey(Type contextType)
    {
        ArgumentNullException.ThrowIfNull(contextType);
        RemoveForeignKeyByContextType[contextType] = true;
    }

    /// <summary>
    /// 重写迁移操作生成逻辑：仅当"当前上下文"启用开关时，跳过建表与新增关系操作的外键生成
    /// </summary>
    /// <param name="operation">迁移操作</param>
    /// <param name="model">EF Core 模型</param>
    /// <param name="builder">迁移命令列表构建器</param>
    protected override void Generate(
        MigrationOperation operation,
        IModel model,
        MigrationCommandListBuilder builder)
    {
        // 迁移生成器为 per-context 作用域服务：优先取注册类型（代理/派生实例不会错位），
        // 无法取得时回退到运行时类型
        var context = Dependencies.CurrentContext.Context;
        var options = context?.GetService<IDbContextOptions>() as DbContextOptions;
        var contextType = options?.ContextType ?? context?.GetType();

        if (contextType == null
            || !RemoveForeignKeyByContextType.TryGetValue(contextType, out var removeForeignKey)
            || !removeForeignKey)
        {
            base.Generate(operation, model, builder);
            return;
        }

        if (operation is CreateTableOperation createTableOperation)
        {
            Dependencies.Logger.Logger.LogDebug("Skip create foreign key for table: {Table}",
                createTableOperation.Name);
            createTableOperation.ForeignKeys.Clear();
        }
        else if (operation is DropForeignKeyOperation dropForeignKeyOperation)
        {
            // 修改关系或回滚迁移时，EF 会生成删除外键操作；但按本框架约定数据库外键从未被创建
            // （建表/新增外键均已跳过），照常生成"删除不存在的外键"SQL 会导致迁移执行失败，
            // 故同样吞掉该操作（不生成任何 SQL）
            Dependencies.Logger.Logger.LogDebug("Skip drop foreign key: {Name}",
                dropForeignKeyOperation.Name);
            return;
        }
        else if (operation is AddForeignKeyOperation addForeignKeyOperation)
        {
            // EF 在“为已有表新增关系”时会生成独立的 AddForeignKeyOperation，
            // 仅清理 CreateTableOperation.ForeignKeys 会漏掉这类外键，导致 schema 混用
            Dependencies.Logger.Logger.LogDebug("Skip add foreign key: {Name}",
                addForeignKeyOperation.Name);
            return;
        }

        base.Generate(operation, model, builder);
    }
}
