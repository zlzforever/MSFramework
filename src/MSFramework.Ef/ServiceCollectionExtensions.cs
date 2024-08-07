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

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddEfAuditing<TDbContext>(this IServiceCollection services)
        where TDbContext : DbContext
    {
        EfUtilities.AuditingDbContextType = typeof(TDbContext);
        services.AddScoped<IAuditingStore, EfAuditingStore<TDbContext>>();
        return services;
    }

    public static MicroserviceFrameworkBuilder UseEfAuditing<TDbContext>(this MicroserviceFrameworkBuilder builder)
        where TDbContext : DbContext
    {
        EfUtilities.AuditingDbContextType = typeof(TDbContext);
        builder.Services.AddScoped<IAuditingStore, EfAuditingStore<TDbContext>>();
        return builder;
    }

    public static MicroserviceFrameworkBuilder UseEntityFramework(this MicroserviceFrameworkBuilder builder)
    {
        builder.Services.UseEntityFramework();
        return builder;
    }

    public static IServiceCollection UseEntityFramework(this IServiceCollection services)
    {
        services.AddMemoryCache();
        services.TryAddSingleton<IEntityConfigurationTypeFinder, EntityConfigurationTypeFinder>();
        services.TryAddScoped<DbContextFactory>();
        services.TryAddScoped<IUnitOfWork, EfUnitOfWork>();
        services.TryAddScoped(typeof(IExternalEntityRepository<,>), typeof(ExternalEntityRepository<,>));
        return services;
    }

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
