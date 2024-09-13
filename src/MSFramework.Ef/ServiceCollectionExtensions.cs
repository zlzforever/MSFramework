using MicroserviceFramework.Auditing;
using MicroserviceFramework.Domain;
using MicroserviceFramework.Ef.Auditing;
using MicroserviceFramework.Ef.Internal;
using MicroserviceFramework.Ef.Repositories;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace MicroserviceFramework.Ef;

/// <summary>
///
/// </summary>
public static class ServiceCollectionExtensions
{
    /// <summary>
    ///
    /// </summary>
    /// <param name="services"></param>
    /// <typeparam name="TDbContext"></typeparam>
    /// <returns></returns>
    public static IServiceCollection AddEfAuditing<TDbContext>(this IServiceCollection services)
        where TDbContext : DbContext
    {
        EfUtilities.AuditingDbContextType = typeof(TDbContext);
        services.AddScoped<IAuditingStore, EfAuditingStore<TDbContext>>();
        return services;
    }

    /// <summary>
    ///
    /// </summary>
    /// <param name="builder"></param>
    /// <typeparam name="TDbContext"></typeparam>
    /// <returns></returns>
    public static MicroserviceFrameworkBuilder UseEfAuditing<TDbContext>(this MicroserviceFrameworkBuilder builder)
        where TDbContext : DbContext
    {
        EfUtilities.AuditingDbContextType = typeof(TDbContext);
        builder.Services.AddScoped<IAuditingStore, EfAuditingStore<TDbContext>>();
        return builder;
    }

    /// <summary>
    ///
    /// </summary>
    /// <param name="builder"></param>
    /// <returns></returns>
    public static MicroserviceFrameworkBuilder UseEntityFramework(this MicroserviceFrameworkBuilder builder)
    {
        builder.Services.AddEntityFrameworkExtension();
        return builder;
    }

    /// <summary>
    ///
    /// </summary>
    /// <param name="services"></param>
    /// <returns></returns>
    public static IServiceCollection AddEntityFrameworkExtension(this IServiceCollection services)
    {
        services.TryAddSingleton<IEntityConfigurationTypeFinder, EntityConfigurationTypeFinder>();
        services.TryAddScoped<DbContextFactory>();
        services.TryAddScoped<IUnitOfWork, EfUnitOfWork>();
        services.TryAddScoped(typeof(IExternalEntityRepository<,>), typeof(ExternalEntityRepository<,>));
        // services.TryAddSingleton<IScopeServiceProvider, NullScopeServiceProvider>();
        return services;
    }

    /// <summary>
    ///
    /// </summary>
    /// <param name="builder"></param>
    /// <param name="connectionString"></param>
    /// <typeparam name="T"></typeparam>
    /// <exception cref="MicroserviceFrameworkException"></exception>
    public static void SetConnectionString<T>(this DbContextOptionsBuilder builder, string connectionString) where T :
        class,
        IDbContextOptionsExtension
    {
#pragma warning disable EF1001
        var extension = builder.Options.FindExtension<T>() as RelationalOptionsExtension;
        if (extension == null)
        {
            throw new MicroserviceFrameworkException("NpgsqlOptionsExtension is null");
        }

        var b = extension.WithConnectionString(connectionString);
#pragma warning restore EF1001
        ((IDbContextOptionsBuilderInfrastructure)builder).AddOrUpdateExtension(b);
    }
}
