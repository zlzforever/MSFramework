using System;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;
using Microsoft.EntityFrameworkCore.SqlServer.Infrastructure.Internal;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;

namespace MicroserviceFramework.Ef.SqlServer;

public static class ServiceCollectionExtensions
{
    public static SqlServerDbContextOptionsBuilder UseRemoveForeignKeyService(
        this SqlServerDbContextOptionsBuilder options)
    {
        MigrationsSqlGenerator.RemoveForeignKey = true;
        var ops = (IRelationalDbContextOptionsBuilderInfrastructure)options;
        ops.OptionsBuilder.ReplaceService<IMigrationsSqlGenerator, MigrationsSqlGenerator>();
        return options;
    }

    public static SqlServerDbContextOptionsBuilder UseRemoveExternalEntityService(
        this SqlServerDbContextOptionsBuilder options)
    {
        MigrationsSqlGenerator.RemoveExternalEntity = true;
        var ops = (IRelationalDbContextOptionsBuilderInfrastructure)options;
        ops.OptionsBuilder.ReplaceService<IMigrationsSqlGenerator, MigrationsSqlGenerator>();
        return options;
    }

    public static void LoadFromConfiguration(this SqlServerDbContextOptionsBuilder builder,
        IServiceProvider provider)
    {
        var dbContextOptionsBuilder = ((IRelationalDbContextOptionsBuilderInfrastructure)builder).OptionsBuilder;
        var contextType = dbContextOptionsBuilder.Options.ContextType;
        var dbContextSettingsList = provider.GetRequiredService<IOptions<DbContextSettingsList>>().Value;
        var option = dbContextSettingsList.Get(contextType);
        var entryAssemblyName = !string.IsNullOrWhiteSpace(option.MigrationsAssembly)
            ? option.MigrationsAssembly
            : contextType.Assembly.GetName().Name;

        var migrationsHistoryTable = string.IsNullOrWhiteSpace(option.TablePrefix)
            ? EfUtilities.MigrationsHistoryTable
            : $"{option.TablePrefix}migrations_history";
        dbContextOptionsBuilder.EnableSensitiveDataLogging(option.EnableSensitiveDataLogging);

#pragma warning disable EF1001
        dbContextOptionsBuilder.SetConnectionString<SqlServerOptionsExtension>(option.ConnectionString);
#pragma warning restore EF1001
        builder.MigrationsHistoryTable(migrationsHistoryTable);
        builder.MaxBatchSize(option.MaxBatchSize);
        builder.MigrationsAssembly(entryAssemblyName);
        builder.UseQuerySplittingBehavior(QuerySplittingBehavior.SplitQuery);
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
