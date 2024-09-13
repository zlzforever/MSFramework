using MicroserviceFramework.Ef.Extensions;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;
using Microsoft.EntityFrameworkCore.SqlServer.Infrastructure.Internal;

namespace MicroserviceFramework.Ef.SqlServer;

/// <summary>
///
/// </summary>
public static class ServiceCollectionExtensions
{
    /// <summary>
    ///
    /// </summary>
    /// <param name="options"></param>
    /// <returns></returns>
    public static SqlServerDbContextOptionsBuilder UseRemoveForeignKeyService(
        this SqlServerDbContextOptionsBuilder options)
    {
        MigrationsSqlGenerator.RemoveForeignKey = true;
        var ops = (IRelationalDbContextOptionsBuilderInfrastructure)options;
        ops.OptionsBuilder.ReplaceService<IMigrationsSqlGenerator, MigrationsSqlGenerator>();
        return options;
    }

    /// <summary>
    ///
    /// </summary>
    /// <param name="options"></param>
    /// <returns></returns>
    public static SqlServerDbContextOptionsBuilder UseRemoveExternalEntityService(
        this SqlServerDbContextOptionsBuilder options)
    {
        MigrationsSqlGenerator.RemoveExternalEntity = true;
        var ops = (IRelationalDbContextOptionsBuilderInfrastructure)options;
        ops.OptionsBuilder.ReplaceService<IMigrationsSqlGenerator, MigrationsSqlGenerator>();
        return options;
    }

    /// <summary>
    ///
    /// </summary>
    /// <param name="builder"></param>
    /// <param name="settings"></param>
    public static void Load(this SqlServerDbContextOptionsBuilder builder,
        DbContextSettings settings)
    {
#pragma warning disable EF1001
        builder.LoadDbContextSettings<SqlServerDbContextOptionsBuilder, SqlServerOptionsExtension>(settings);
        var dbContextOptionsBuilder = ((IRelationalDbContextOptionsBuilderInfrastructure)builder).OptionsBuilder;
        dbContextOptionsBuilder.SetConnectionString<SqlServerOptionsExtension>(settings.ConnectionString);
#pragma warning restore EF1001
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
