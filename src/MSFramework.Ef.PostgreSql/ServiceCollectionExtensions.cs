using System;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Npgsql.EntityFrameworkCore.PostgreSQL.Infrastructure;
using Npgsql.EntityFrameworkCore.PostgreSQL.Infrastructure.Internal;

namespace MicroserviceFramework.Ef.PostgreSql;

public static class ServiceCollectionExtensions
{
    public static NpgsqlDbContextOptionsBuilder UseRemoveForeignKeyService(this NpgsqlDbContextOptionsBuilder options)
    {
        MigrationsSqlGenerator.RemoveForeignKey = true;
        var ops = (IRelationalDbContextOptionsBuilderInfrastructure)options;
        ops.OptionsBuilder.ReplaceService<IMigrationsSqlGenerator, MigrationsSqlGenerator>();
        return options;
    }

    public static NpgsqlDbContextOptionsBuilder UseRemoveExternalEntityService(
        this NpgsqlDbContextOptionsBuilder options)
    {
        MigrationsSqlGenerator.RemoveExternalEntity = true;
        var ops = (IRelationalDbContextOptionsBuilderInfrastructure)options;
        ops.OptionsBuilder.ReplaceService<IMigrationsSqlGenerator, MigrationsSqlGenerator>();
        return options;
    }

    public static void LoadFromConfiguration(this NpgsqlDbContextOptionsBuilder builder,
        IServiceProvider provider)
    {
        var dbContextSettingsList = provider.GetRequiredService<IOptions<DbContextSettingsList>>().Value;
        var dbContextOptionsBuilder = ((IRelationalDbContextOptionsBuilderInfrastructure)builder).OptionsBuilder;
        var contextType = dbContextOptionsBuilder.Options.ContextType;

        var option = dbContextSettingsList.Get(contextType);
        var entryAssemblyName = !string.IsNullOrWhiteSpace(option.MigrationsAssembly)
            ? option.MigrationsAssembly
            : contextType.Assembly.GetName().Name;

        var migrationsHistoryTable = string.IsNullOrWhiteSpace(option.TablePrefix)
            ? EfUtilities.MigrationsHistoryTable
            : $"{option.TablePrefix}migrations_history";
        dbContextOptionsBuilder.EnableSensitiveDataLogging(option.EnableSensitiveDataLogging);

#pragma warning disable EF1001
        dbContextOptionsBuilder.SetConnectionString<NpgsqlOptionsExtension>(option.ConnectionString);
#pragma warning restore EF1001
        builder.MigrationsHistoryTable(migrationsHistoryTable);
        builder.MaxBatchSize(option.MaxBatchSize);
        builder.MigrationsAssembly(entryAssemblyName);
        builder.UseQuerySplittingBehavior(QuerySplittingBehavior.SplitQuery);
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
