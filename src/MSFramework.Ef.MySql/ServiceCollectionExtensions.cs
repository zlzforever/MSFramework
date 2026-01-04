using MicroserviceFramework.Ef.Extensions;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;
using Pomelo.EntityFrameworkCore.MySql.Infrastructure.Internal;

namespace MicroserviceFramework.Ef.MySql;

/// <summary>
///
/// </summary>
public static class ServiceCollectionExtensions
{
    /// <param name="options"></param>
    extension(MySqlDbContextOptionsBuilder options)
    {
        /// <summary>
        ///
        /// </summary>
        /// <returns></returns>
        public MySqlDbContextOptionsBuilder UseRemoveForeignKeyService()
        {
            MigrationsSqlGenerator.RemoveForeignKey = true;
            var ops = (IRelationalDbContextOptionsBuilderInfrastructure)options;
            ops.OptionsBuilder.ReplaceService<IMigrationsSqlGenerator, MigrationsSqlGenerator>();
            return options;
        }

        /// <summary>
        ///
        /// </summary>
        /// <returns></returns>
        public MySqlDbContextOptionsBuilder UseRemoveExternalEntityService()
        {
            MigrationsSqlGenerator.RemoveExternalEntity = true;
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
    ///
    /// </summary>
    /// <param name="builder"></param>
    /// <param name="settings"></param>
    public static void Load(this MySqlDbContextOptionsBuilder builder,
        DbContextSettings settings)
    {
#pragma warning disable EF1001
        var dbContextOptionsBuilder = ((IRelationalDbContextOptionsBuilderInfrastructure)builder).OptionsBuilder;
        builder.LoadDbContextSettings<MySqlDbContextOptionsBuilder, MySqlOptionsExtension>(settings);
        dbContextOptionsBuilder.SetConnectionString<MySqlOptionsExtension>(settings.ConnectionString);
#pragma warning restore EF1001
    }

    // public static EntityFrameworkBuilder AddMySql<TDbContext>(
    //     this EntityFrameworkBuilder builder, IConfiguration configuration,
    //     Action<DbContextOptionsBuilder, MySqlDbContextOptionsBuilder> mySqlOptionsAction = null)
    //     where TDbContext : DbContextBase
    // {
    //     builder.Services.AddMySql<TDbContext>(configuration, mySqlOptionsAction);
    //     return builder;
    // }
    //
    // public static EntityFrameworkBuilder AddMySql<TDbContext1, TDbContext2>(
    //     this EntityFrameworkBuilder builder, IConfiguration configuration,
    //     Action<DbContextOptionsBuilder, MySqlDbContextOptionsBuilder> mySqlOptionsAction = null)
    //     where TDbContext1 : DbContextBase
    //     where TDbContext2 : DbContextBase
    // {
    //     builder.Services.AddMySql<TDbContext1>(configuration, mySqlOptionsAction);
    //     builder.Services.AddMySql<TDbContext2>(configuration, mySqlOptionsAction);
    //     return builder;
    // }
    //
    // public static EntityFrameworkBuilder AddMySql<TDbContext1, TDbContext2, TDbContext3>(
    //     this EntityFrameworkBuilder builder, IConfiguration configuration,
    //     Action<DbContextOptionsBuilder, MySqlDbContextOptionsBuilder> mySqlOptionsAction = null)
    //     where TDbContext1 : DbContextBase
    //     where TDbContext2 : DbContextBase
    //     where TDbContext3 : DbContextBase
    // {
    //     builder.Services.AddMySql<TDbContext1>(configuration, mySqlOptionsAction);
    //     builder.Services.AddMySql<TDbContext2>(configuration, mySqlOptionsAction);
    //     builder.Services.AddMySql<TDbContext3>(configuration, mySqlOptionsAction);
    //
    //     return builder;
    // }

    // public static IServiceCollection AddMySql<TDbContext>(
    //     this IServiceCollection services, IConfiguration configuration,
    //     Action<DbContextOptionsBuilder, MySqlDbContextOptionsBuilder> mySqlOptionsAction = null)
    //     where TDbContext : DbContextBase
    // {
    //     var action = new Action<DbContextOptionsBuilder>(x =>
    //     {
    //         var dbContextType = typeof(TDbContext);
    //         var optionDict = configuration.GetSection("DbContexts").Get<DbContextSettingsDict>();
    //         var option = optionDict.Get(dbContextType);
    //
    //         var entryAssemblyName = !string.IsNullOrWhiteSpace(option.MigrationsAssembly)
    //             ? option.MigrationsAssembly
    //             : dbContextType.Assembly.GetName().Name;
    //
    //         x.UseMySql(option.ConnectionString, ServerVersion.AutoDetect(option.ConnectionString), options =>
    //         {
    //             var migrationsHistoryTable = string.IsNullOrWhiteSpace(option.TablePrefix)
    //                 ? EfUtilities.MigrationsHistoryTable
    //                 : $"{option.TablePrefix}migrations_history";
    //
    //             options.MigrationsHistoryTable(migrationsHistoryTable);
    //             options.MaxBatchSize(option.MaxBatchSize);
    //             options.MigrationsAssembly(entryAssemblyName);
    //             options.SchemaBehavior(MySqlSchemaBehavior.Translate, (schema, table) => $"{schema}_{table}");
    //             options.UseQuerySplittingBehavior(QuerySplittingBehavior.SplitQuery);
    //
    //             // 不管默认行为是怎么样的， 代码配置是保底的
    //             mySqlOptionsAction?.Invoke(x, options);
    //         });
    //     });
    //     services.AddDbContext<TDbContext>(action);
    //     return services;
    // }
}
