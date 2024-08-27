using System.Reflection;
using MicroserviceFramework.Domain;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace MicroserviceFramework.AutoMapper;

public static class ServiceCollectionExtensions
{
    public static MicroserviceFrameworkBuilder UseAutoMapperObjectAssembler(this MicroserviceFrameworkBuilder builder)
    {
        builder.Services.AddAutoMapperObjectAssembler();
        return builder;
    }

    public static IServiceCollection AddAutoMapperObjectAssembler(this IServiceCollection services)
    {
        services.TryAddScoped<IObjectAssembler, AutoMapperObjectAssembler>();
        services.AddAutoMapper(Utils.Runtime.GetAllAssemblies());
        return services;
    }

    public static MicroserviceFrameworkBuilder UseAutoMapperObjectAssembler(this MicroserviceFrameworkBuilder builder,
        params Assembly[] assemblies)
    {
        builder.Services.TryAddScoped<IObjectAssembler, AutoMapperObjectAssembler>();
        builder.Services.AddAutoMapper(assemblies);
        return builder;
    }
}
