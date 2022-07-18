using System;
using MicroserviceFramework.AspNetCore.DependencyInjection;
using MicroserviceFramework.AspNetCore.Infrastructure;
using MicroserviceFramework.AspNetCore.Mvc.ModelBinding;
using MicroserviceFramework.Extensions.DependencyInjection;
using MicroserviceFramework.Runtime;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Infrastructure;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Options;
using ISession = MicroserviceFramework.Application.ISession;

namespace MicroserviceFramework.AspNetCore;

public static class ServiceCollectionExtensions
{
    /// <summary>
    /// 
    /// </summary>
    /// <param name="builder"></param>
    /// <returns></returns>
    public static MicroserviceFrameworkBuilder UseAspNetCore(this MicroserviceFrameworkBuilder builder)
    {
        builder.Services.AddHttpContextAccessor();
        builder.Services.AddSingleton<IActionResultTypeMapper, ActionResultTypeMapper>();
        builder.Services.AddSingleton<IScopedServiceResolver, ScopedServiceResolver>();
        builder.Services.TryAddScoped<ISession, HttpSession>();

        builder.Services.AddSingleton(
            x => x.GetRequiredService<IOptions<JsonOptions>>().Value.JsonSerializerOptions);

        var jsonSerializerSettingsType = Type.GetType("Newtonsoft.Json.JsonSerializerSettings, Newtonsoft.Json");
        var jsonOptionsType =
            Type.GetType(
                "Microsoft.AspNetCore.Mvc.MvcNewtonsoftJsonOptions, Microsoft.AspNetCore.Mvc.NewtonsoftJson");
        if (jsonSerializerSettingsType != null && jsonOptionsType != null)
        {
            builder.Services.AddSingleton(jsonSerializerSettingsType, (x) =>
            {
                var type = typeof(IOptions<>).MakeGenericType(jsonOptionsType);
                return ((dynamic)x.GetRequiredService(type)).Value.SerializerSettings;
            });
        }

        return builder;
    }

    public static void UseMicroserviceFramework(this IApplicationBuilder builder)
    {
        builder.ApplicationServices.UseMicroserviceFramework();
    }

    public static IMvcBuilder ConfigureInvalidModelStateResponse(this IMvcBuilder builder)
    {
        builder.ConfigureApiBehaviorOptions(x =>
        {
            x.InvalidModelStateResponseFactory = InvalidModelStateResponseFactory.Instance;
        });
        return builder;
    }

    public static MicroserviceFrameworkBuilder UseAssemblyScanPrefix(this MicroserviceFrameworkBuilder builder,
        params string[] prefixes)
    {
        RuntimeUtilities.StartsWith.AddRange(prefixes);
        return builder;
    }
}
