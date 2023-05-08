using System;
using System.Linq;
using System.Reflection;
using MicroserviceFramework.Domain;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace MicroserviceFramework.AutoMapper;

public static class ServiceCollectionExtensions
{
    public static MicroserviceFrameworkBuilder UseAutoMapper(this MicroserviceFrameworkBuilder builder)
    {
        return builder.UseAutoMapper( Utils.Runtime.GetAllAssemblies());
    }

    public static MicroserviceFrameworkBuilder UseAutoMapper(this MicroserviceFrameworkBuilder builder,
        params Assembly[] assemblies)
    {
        builder.Services.TryAddScoped<IObjectAssembler, AutoMapperObjectAssembler>();
        builder.Services.AddAutoMapper(assemblies);
        return builder;
    }

    public static MicroserviceFrameworkBuilder UseAutoMapper(this MicroserviceFrameworkBuilder builder,
        params Type[] types)
    {
        return builder.UseAutoMapper(types.Select(x => x.Assembly).ToArray());
    }
}
