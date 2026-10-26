using System;
using MicroserviceFramework.Ef.Extensions;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Infrastructure;
using Npgsql.EntityFrameworkCore.PostgreSQL.Infrastructure.Internal;

namespace MicroserviceFramework.Ef.PostgreSql;

/// <summary>
/// PostgreSQL 数据库提供程序的 ServiceCollection 扩展方法
/// </summary>
public static class ServiceCollectionExtensions
{
    /// <param name="options"></param>
    extension(NpgsqlDbContextOptionsBuilder options)
    {
        /// <summary>
        /// PostgreSQL 迁移不生成数据库外键：本框架统一约定不使用数据库外键，
        /// 引用完整性由业务代码保证（配合软删除 ClientCascade 约定，禁止数据库级联删除）。
        /// 设置按上下文隔离的迁移生成器开关并替换为框架定制生成器，跳过建表/新增关系操作的外键生成；
        /// 开关仅对当前 OptionsBuilder 所属的 DbContext 生效，不影响同进程其他上下文
        /// </summary>
        /// <returns>原 PostgreSQL 选项构建器，支持链式调用</returns>
        public NpgsqlDbContextOptionsBuilder UseNoForeignKeyMigration()
        {
            var ops = (IRelationalDbContextOptionsBuilderInfrastructure)options;
            // 以当前 OptionsBuilder 所属上下文类型为键写入开关，避免跨上下文泄漏
            var contextType = ops.OptionsBuilder.Options.ContextType
                              ?? throw new InvalidOperationException(
                                  "UseNoForeignKeyMigration 需要在 AddDbContext/AddDbContextPool 的 optionsAction 中调用，" +
                                  "以便获取当前 DbContext 类型");
            MigrationsSqlGenerator.SetRemoveForeignKey(contextType);
            ops.OptionsBuilder.ReplaceService<IMigrationsSqlGenerator, MigrationsSqlGenerator>();
            return options;
        }

        /// <summary>
        /// 旧名称，保留转发以兼容既有调用；转发后统一启用无外键迁移（旧行为未设置开关，PostgreSQL 仍生成外键）。
        /// </summary>
        /// <returns>原 PostgreSQL 选项构建器，支持链式调用</returns>
        [Obsolete("改为 UseNoForeignKeyMigration：统一启用无外键迁移")]
        public NpgsqlDbContextOptionsBuilder UseRemoveForeignKeyService()
        {
            return options.UseNoForeignKeyMigration();
        }
    }

    /// <summary>
    /// 从 DbContextSettings 加载配置并应用到 NpgsqlDbContextOptionsBuilder
    /// </summary>
    /// <param name="builder">PostgreSQL 数据库上下文选项构建器</param>
    /// <param name="settings">数据库上下文配置</param>
    public static void Load(this NpgsqlDbContextOptionsBuilder builder,
        DbContextSettings settings)
    {
#pragma warning disable EF1001
        builder.LoadDbContextSettings<NpgsqlDbContextOptionsBuilder, NpgsqlOptionsExtension>(settings);
        var dbContextOptionsBuilder = ((IRelationalDbContextOptionsBuilderInfrastructure)builder).OptionsBuilder;
        dbContextOptionsBuilder.SetConnectionString<NpgsqlOptionsExtension>(settings.ConnectionString);
#pragma warning restore EF1001
        builder.UseNoForeignKeyMigration();
    }

    // public static IServiceCollection AddNpgsql<TDbContext>(
    //     this IServiceCollection services, IConfiguration configuration,
    //     Action<NpgsqlDbContextOptionsBuilder> configure = null) where TDbContext : DbContextBase
    // {
    //     var action = new Action<DbContextOptionsBuilder>(x =>
    //     {
    //         var dbContextType = typeof(TDbContext);
    //
    //         var optionDict = configuration.GetSection("DbContexts").Get<DbContextSettingsDict>();
    //         var option = optionDict.Get(dbContextType);
    //         var entryAssemblyName = !string.IsNullOrWhiteSpace(option.MigrationsAssembly)
    //             ? option.MigrationsAssembly
    //             : dbContextType.Assembly.GetName().Name;
    //
    //         x.UseNpgsql(option.ConnectionString, options =>
    //             {
    //                 var migrationsHistoryTable = string.IsNullOrWhiteSpace(option.TablePrefix)
    //                     ? EfUtilities.MigrationsHistoryTable
    //                     : $"{option.TablePrefix}migrations_history";
    //                 options.MigrationsHistoryTable(migrationsHistoryTable);
    //                 options.MaxBatchSize(option.MaxBatchSize);
    //                 options.MigrationsAssembly(entryAssemblyName);
    //                 options.UseQuerySplittingBehavior(QuerySplittingBehavior.SplitQuery);
    //
    //                 configure?.Invoke(options);
    //             })
    //             ;
    //     });
    //
    //     services.AddDbContext<TDbContext>(action);
    //     return services;
    // }
}
