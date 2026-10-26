using System;
using MicroserviceFramework.Ef.Extensions;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;
using Microsoft.EntityFrameworkCore.SqlServer.Infrastructure.Internal;

namespace MicroserviceFramework.Ef.SqlServer;

/// <summary>
/// SQL Server 数据库提供程序的 ServiceCollection 扩展方法
/// </summary>
public static class ServiceCollectionExtensions
{
    /// <param name="options"></param>
    extension(SqlServerDbContextOptionsBuilder options)
    {
        /// <summary>
        /// SQL Server 迁移不生成数据库外键：本框架统一约定不使用数据库外键，
        /// 引用完整性由业务代码保证（配合软删除 ClientCascade 约定，禁止数据库级联删除）。
        /// 设置按上下文隔离的迁移生成器开关并替换为框架定制生成器，跳过建表/新增关系操作的外键生成；
        /// 开关仅对当前 OptionsBuilder 所属的 DbContext 生效，不影响同进程其他上下文
        /// </summary>
        /// <returns>原 SQL Server 选项构建器，支持链式调用</returns>
        public SqlServerDbContextOptionsBuilder UseNoForeignKeyMigration()
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
    }

    /// <summary>
    /// 从 DbContextSettings 加载配置并应用到 SqlServerDbContextOptionsBuilder
    /// </summary>
    /// <param name="builder">SQL Server 数据库上下文选项构建器</param>
    /// <param name="settings">数据库上下文配置</param>
    public static void Load(this SqlServerDbContextOptionsBuilder builder,
        DbContextSettings settings)
    {
#pragma warning disable EF1001
        builder.LoadDbContextSettings<SqlServerDbContextOptionsBuilder, SqlServerOptionsExtension>(settings);
        var dbContextOptionsBuilder = ((IRelationalDbContextOptionsBuilderInfrastructure)builder).OptionsBuilder;
        dbContextOptionsBuilder.SetConnectionString<SqlServerOptionsExtension>(settings.ConnectionString);
#pragma warning restore EF1001
        builder.UseNoForeignKeyMigration();
    }

    // public static EntityFrameworkBuilder AddSqlServer<TDbContext>(
    //     this EntityFrameworkBuilder builder, IConfiguration configuration,
    //     Action<SqlServerDbContextOptionsBuilder> sqlServerOptionsAction = null) where TDbContext : DbContextBase
    // {
    //     builder.Services.AddDbContext<TDbContext>(x =>
    //     {
    //         var dbContextType = typeof(TDbContext);
    //         var optionDict = configuration.GetSection("DbContexts").Get<DbContextSettingsDict>();
    //         var option = optionDict.Get(dbContextType);
    //         var entryAssemblyName = !string.IsNullOrWhiteSpace(option.MigrationsAssembly)
    //             ? option.MigrationsAssembly
    //             : dbContextType.Assembly.GetName().Name;
    //         x.UseSqlServer(option.ConnectionString, options =>
    //         {
    //             var migrationsHistoryTable = string.IsNullOrWhiteSpace(option.TablePrefix)
    //                 ? EfUtilities.MigrationsHistoryTable
    //                 : $"{option.TablePrefix}migrations_history";
    //             options.MigrationsHistoryTable(migrationsHistoryTable);
    //             options.MaxBatchSize(option.MaxBatchSize);
    //             options.MigrationsAssembly(entryAssemblyName);
    //             options.UseQuerySplittingBehavior(QuerySplittingBehavior.SplitQuery);
    //
    //             sqlServerOptionsAction?.Invoke(options);
    //         });
    //     });
    //     builder.Services.AddScoped<DbContext, TDbContext>();
    //     return builder;
    // }
}
