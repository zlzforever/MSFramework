using MicroserviceFramework.Ef.Extensions;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Infrastructure;
using Npgsql.EntityFrameworkCore.PostgreSQL.Infrastructure.Internal;

namespace MicroserviceFramework.Ef.PostgreSql;

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
    public static NpgsqlDbContextOptionsBuilder UseRemoveForeignKeyService(this NpgsqlDbContextOptionsBuilder options)
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
    public static NpgsqlDbContextOptionsBuilder UseRemoveExternalEntityService(
        this NpgsqlDbContextOptionsBuilder options)
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
    public static void Load(this NpgsqlDbContextOptionsBuilder builder,
        DbContextSettings settings)
    {
#pragma warning disable EF1001
        builder.LoadDbContextSettings<NpgsqlDbContextOptionsBuilder, NpgsqlOptionsExtension>(settings);
        var dbContextOptionsBuilder = ((IRelationalDbContextOptionsBuilderInfrastructure)builder).OptionsBuilder;
        dbContextOptionsBuilder.SetConnectionString<NpgsqlOptionsExtension>(settings.ConnectionString);
#pragma warning restore EF1001
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
