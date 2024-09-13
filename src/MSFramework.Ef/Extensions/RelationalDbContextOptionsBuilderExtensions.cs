using System;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;

namespace MicroserviceFramework.Ef.Extensions;

/// <summary>
///
/// </summary>
public static class RelationalDbContextOptionsBuilderExtensions
{
    internal static void LoadDbContextSettings<TDbContextOptionsBuilder, TOptionsExtension>(
        this TDbContextOptionsBuilder builder, DbContextSettings settings)
        where TDbContextOptionsBuilder : RelationalDbContextOptionsBuilder<TDbContextOptionsBuilder, TOptionsExtension>
        where TOptionsExtension : RelationalOptionsExtension, new()
    {
        var dbContextOptionsBuilder = ((IRelationalDbContextOptionsBuilderInfrastructure)builder).OptionsBuilder;
        ((IDbContextOptionsBuilderInfrastructure)dbContextOptionsBuilder).AddOrUpdateExtension(settings);

        dbContextOptionsBuilder.ConfigureLoggingCacheTime(TimeSpan.FromSeconds(settings.LoggingCacheTime));
        dbContextOptionsBuilder.EnableSensitiveDataLogging(settings.EnableSensitiveDataLogging);
        dbContextOptionsBuilder.EnableDetailedErrors(settings.EnableDetailedErrors);
        dbContextOptionsBuilder.EnableServiceProviderCaching(settings.EnableServiceProviderCaching);
        dbContextOptionsBuilder.EnableThreadSafetyChecks(settings.EnableThreadSafetyChecks);

        var migrationsHistoryTable = settings.GetMigrationsHistoryTable();
        var contextType = dbContextOptionsBuilder.Options.ContextType;
        var entryAssemblyName = !string.IsNullOrWhiteSpace(settings.MigrationsAssembly)
            ? settings.MigrationsAssembly
            : contextType.Assembly.GetName().Name;
        var querySplittingBehavior =
            "SplitQuery".Equals(settings.QuerySplittingBehavior, StringComparison.OrdinalIgnoreCase)
                ? QuerySplittingBehavior.SplitQuery
                : QuerySplittingBehavior.SingleQuery;

        builder.MaxBatchSize(settings.MaxBatchSize);
        builder.CommandTimeout(settings.CommandTimeout);
        builder.MigrationsHistoryTable(migrationsHistoryTable);
        builder.MigrationsAssembly(entryAssemblyName);
        builder.UseQuerySplittingBehavior(querySplittingBehavior);
    }
}
