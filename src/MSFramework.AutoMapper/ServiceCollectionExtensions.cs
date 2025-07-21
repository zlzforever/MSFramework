using System.Reflection;
using MicroserviceFramework.Domain;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace MicroserviceFramework.AutoMapper;

/// <summary>
///
/// </summary>
public static class ServiceCollectionExtensions
{
    /// <summary>
    ///
    /// </summary>
    /// <param name="builder"></param>
    /// <returns></returns>
    public static MicroserviceFrameworkBuilder UseAutoMapperObjectAssembler(this MicroserviceFrameworkBuilder builder)
    {
        builder.Services.AddAutoMapperObjectAssembler();
        return builder;
    }

    /// <summary>
    ///
    /// </summary>
    /// <param name="services"></param>
    /// <returns></returns>
    public static IServiceCollection AddAutoMapperObjectAssembler(this IServiceCollection services)
    {
        services.TryAddScoped<IObjectAssembler, AutoMapperObjectAssembler>();
        services.AddAutoMapper(Utils.Runtime.GetAllAssemblies());
        return services;
    }

    /// <summary>
    ///
    /// </summary>
    /// <param name="builder"></param>
    /// <param name="assemblies"></param>
    /// <returns></returns>
    public static MicroserviceFrameworkBuilder UseAutoMapperObjectAssembler(this MicroserviceFrameworkBuilder builder,
        params Assembly[] assemblies)
    {
        builder.Services.TryAddScoped<IObjectAssembler, AutoMapperObjectAssembler>();
        builder.Services.AddAutoMapper( assemblies);
        return builder;
    }
}
