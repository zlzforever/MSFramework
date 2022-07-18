using System;
using MicroserviceFramework.Domain;
using MicroserviceFramework.Ef.Internal;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace MicroserviceFramework.Ef;

public static class ServiceCollectionExtensions
{
    public static MicroserviceFrameworkBuilder UseEntityFramework(this MicroserviceFrameworkBuilder builder,
        Action<EntityFrameworkBuilder> configure)
    {
        var eBuilder = new EntityFrameworkBuilder(builder.Services);
        configure?.Invoke(eBuilder);
        builder.Services.UseEntityFramework();
        return builder;
    }

    public static IServiceCollection UseEntityFramework(this IServiceCollection services)
    {
        services.AddMemoryCache();
        services.TryAddSingleton<IEntityConfigurationTypeFinder, EntityConfigurationTypeFinder>();
        services.TryAddScoped<DbContextFactory>();
        services.TryAddScoped<IUnitOfWork, EfUnitOfWork>();
        return services;
    }
}
