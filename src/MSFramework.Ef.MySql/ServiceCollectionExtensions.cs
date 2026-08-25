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
        // /// <summary>
        // ///
        // /// </summary>
        // /// <returns></returns>
        // public MySqlDbContextOptionsBuilder UseRemoveForeignKeyService()
        // {
        //     // MigrationsSqlGenerator.RemoveForeignKey = true;
        //     // Console.WriteLine("Set MigrationsSqlGenerator.RemoveForeignKey ");
        //     // var ops = (IRelationalDbContextOptionsBuilderInfrastructure)options;
        //     // ops.OptionsBuilder.ReplaceService<IMigrationsSqlGenerator, MigrationsSqlGenerator>();
        //
        //     return options;
        // }

        /// <summary>
        ///
        /// </summary>
        /// <returns></returns>
        public MySqlDbContextOptionsBuilder UseRemoveExternalEntityService()
        {
            MigrationsSqlGenerator.RemoveForeignKey = true;
            var ops = (IRelationalDbContextOptionsBuilderInfrastructure)options;
            ops.OptionsBuilder.ReplaceService<IMigrationsSqlGenerator, MigrationsSqlGenerator>();
            return options;
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
        builder.UseRemoveExternalEntityService();
        // 替换 MigrationsModelDiffer 服务
    }
}
