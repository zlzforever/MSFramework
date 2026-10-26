using System;
using MicroserviceFramework.Ef.Extensions;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;
using Microting.EntityFrameworkCore.MySql.Infrastructure.Internal;

namespace MicroserviceFramework.Ef.MySql;

/// <summary>
/// MySQL 数据库提供程序的 ServiceCollection 扩展方法
/// </summary>
public static class ServiceCollectionExtensions
{
    /// <param name="options"></param>
    extension(MySqlDbContextOptionsBuilder options)
    {
        /// <summary>
        /// MySQL 迁移不生成数据库外键：本框架统一约定不使用数据库外键，
        /// 引用完整性由业务代码保证（配合软删除 ClientCascade 约定，禁止数据库级联删除）。
        /// 设置按上下文隔离的迁移生成器开关并替换为框架定制生成器，跳过建表/新增关系操作的外键生成；
        /// 开关仅对当前 OptionsBuilder 所属的 DbContext 生效，不影响同进程其他上下文
        /// </summary>
        /// <returns>原 MySQL 选项构建器，支持链式调用</returns>
        public MySqlDbContextOptionsBuilder UseNoForeignKeyMigration()
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
        /// 旧名称，实际语义为"迁移不生成数据库外键"（并非仅移除外部实体），保留转发以兼容既有调用。
        /// </summary>
        /// <returns>原 MySQL 选项构建器，支持链式调用</returns>
        [Obsolete("改为 UseNoForeignKeyMigration：本方法实际清除的是全部建表外键，而非仅外部实体")]
        public MySqlDbContextOptionsBuilder UseRemoveExternalEntityService()
        {
            return options.UseNoForeignKeyMigration();
        }
    }

    // public static DbContextOptionsBuilder UseMySql(
    //     this DbContextOptionsBuilder optionsBuilder, IServiceProvider provider,
    //     Action<MySqlDbContextOptionsBuilder> mySqlOptionsAction = null)
    // {
    //     var contextType = optionsBuilder.Options.ContextType;
    //     var dbContextSettingsList = provider.GetRequiredService<IOptions<DbContextSettingsList>>().Value;
    //     var option = dbContextSettingsList.Get(contextType);
    //     optionsBuilder.UseMySql(ServerVersion.AutoDetect(option.ConnectionString), mySqlOptionsAction);
    //     return optionsBuilder;
    // }

    /// <summary>
    /// 从 DbContextSettings 加载配置并应用到 MySqlDbContextOptionsBuilder
    /// </summary>
    /// <param name="builder">MySQL 数据库上下文选项构建器</param>
    /// <param name="settings">数据库上下文配置</param>
    public static void Load(this MySqlDbContextOptionsBuilder builder,
        DbContextSettings settings)
    {
#pragma warning disable EF1001
        var dbContextOptionsBuilder = ((IRelationalDbContextOptionsBuilderInfrastructure)builder).OptionsBuilder;
        builder.LoadDbContextSettings<MySqlDbContextOptionsBuilder, MySqlOptionsExtension>(settings);
        dbContextOptionsBuilder.SetConnectionString<MySqlOptionsExtension>(settings.ConnectionString);
#pragma warning restore EF1001
        builder.UseNoForeignKeyMigration();
        // 替换 MigrationsModelDiffer 服务
    }
}
