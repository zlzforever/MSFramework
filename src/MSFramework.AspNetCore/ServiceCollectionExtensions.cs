using System;
using System.IO;
using System.Reflection;
using MicroserviceFramework.AspNetCore.Mvc.ModelBinding;
using MicroserviceFramework.Extensions.DependencyInjection;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Options;
using ISession = MicroserviceFramework.Application.ISession;

namespace MicroserviceFramework.AspNetCore;

/// <summary>
///
/// </summary>
public static class ServiceCollectionExtensions
{
    /// <summary>
    ///
    /// </summary>
    /// <param name="services"></param>
    /// <returns></returns>
    public static IServiceCollection AddScopeServiceProvider(this IServiceCollection services)
    {
        services.AddSingleton<IScopeServiceProvider, HttpContextScopeServiceProvider>();
        return services;
    }

    /// <summary>
    ///
    /// </summary>
    /// <param name="builder"></param>
    /// <returns></returns>
    public static MicroserviceFrameworkBuilder UseScopeServiceProvider(this MicroserviceFrameworkBuilder builder)
    {
        builder.Services.AddScopeServiceProvider();
        return builder;
    }

    /// <summary>
    ///
    /// </summary>
    /// <param name="services"></param>
    /// <returns></returns>
    public static IServiceCollection AddAspNetCoreExtension(this IServiceCollection services)
    {
        services.TryAddScoped<ISession>(provider =>
            HttpSession.Create(provider.GetRequiredService<IHttpContextAccessor>()));
        services.TryAddSingleton(x =>
            x.GetRequiredService<IOptions<JsonOptions>>().Value.JsonSerializerOptions);

        var file = "Microsoft.AspNetCore.Mvc.NewtonsoftJson.dll";
        if (File.Exists(file))
        {
            Assembly.LoadFrom(file);
            var jsonOptionsType =
                Type.GetType(
                    "Microsoft.AspNetCore.Mvc.MvcNewtonsoftJsonOptions, Microsoft.AspNetCore.Mvc.NewtonsoftJson");
            var jsonSerializerSettingsType = Type.GetType("Newtonsoft.Json.JsonSerializerSettings, Newtonsoft.Json");

            if (jsonSerializerSettingsType != null && jsonOptionsType != null)
            {
                services.TryAddSingleton(jsonSerializerSettingsType, (x) =>
                {
                    var type = typeof(IOptions<>).MakeGenericType(jsonOptionsType);
                    return ((dynamic)x.GetRequiredService(type)).Value.SerializerSettings;
                });
            }
        }

        if (!Directory.Exists(Defaults.OSSDirectory))
        {
            Directory.CreateDirectory(Defaults.OSSDirectory);
        }

        return services;
    }

    /// <summary>
    ///
    /// </summary>
    /// <param name="builder"></param>
    /// <returns></returns>
    public static MicroserviceFrameworkBuilder UseAspNetCoreExtension(this MicroserviceFrameworkBuilder builder)
    {
        builder.Services.AddAspNetCoreExtension();
        return builder;
    }

    /// <summary>
    ///
    /// </summary>
    /// <param name="builder"></param>
    /// <returns></returns>
    public static IApplicationBuilder UseMicroserviceFramework(this IApplicationBuilder builder)
    {
        builder.ApplicationServices.UseMicroserviceFramework();
        return builder;
    }

    /// <summary>
    ///
    /// </summary>
    /// <param name="builder"></param>
    /// <returns></returns>
    public static IMvcBuilder ConfigureInvalidModelStateResponse(this IMvcBuilder builder)
    {
        builder.ConfigureApiBehaviorOptions(x =>
        {
            x.InvalidModelStateResponseFactory = InvalidModelStateResponseFactory.Instance;
        });
        return builder;
    }
}
