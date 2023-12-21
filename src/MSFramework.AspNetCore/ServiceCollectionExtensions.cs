using System;
using MicroserviceFramework.AspNetCore.Mvc.ModelBinding;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Options;
using ISession = MicroserviceFramework.Application.ISession;

namespace MicroserviceFramework.AspNetCore;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddAspNetCore(this IServiceCollection services)
    {
        services.AddHttpContextAccessor();
        services.TryAddScoped<ISession>(provider =>
            HttpSession.Create(provider.GetRequiredService<IHttpContextAccessor>()));
        services.TryAddSingleton(x =>
            x.GetRequiredService<IOptions<JsonOptions>>().Value.JsonSerializerOptions);

        var jsonSerializerSettingsType = Type.GetType("Newtonsoft.Json.JsonSerializerSettings, Newtonsoft.Json");
        var jsonOptionsType =
            Type.GetType(
                "Microsoft.AspNetCore.Mvc.MvcNewtonsoftJsonOptions, Microsoft.AspNetCore.Mvc.NewtonsoftJson");
        if (jsonSerializerSettingsType != null && jsonOptionsType != null)
        {
            services.TryAddSingleton(jsonSerializerSettingsType, (x) =>
            {
                var type = typeof(IOptions<>).MakeGenericType(jsonOptionsType);
                return ((dynamic)x.GetRequiredService(type)).Value.SerializerSettings;
            });
        }

        return services;
    }

    /// <summary>
    ///
    /// </summary>
    /// <param name="builder"></param>
    /// <returns></returns>
    public static MicroserviceFrameworkBuilder UseAspNetCore(this MicroserviceFrameworkBuilder builder)
    {
        builder.Services.AddAspNetCore();
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
}
