using System;
using System.Linq;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace MicroserviceFramework.LocalEvent;

/// <summary>
///
/// </summary>
public static class ServiceCollectionExtensions
{
    /// <summary>
    /// 不适合对所有 IServiceCollection 开放，若没有 Utils.Runtime 支持，注册不进去
    /// </summary>
    /// <param name="services"></param>
    /// <returns></returns>
    private static void AddLocalEventPublisher(this IServiceCollection services)
    {
        services.TryAddScoped<IEventPublisher, LocalEventPublisher>();
        services.AddHostedService<LocalEventBackgroundService>();

        var handlerInterface = typeof(IEventHandler<>);

        var store = new EventDescriptorStore();
        foreach (var type in Utils.Runtime.GetAllTypes())
        {
            var serviceTypes = type
                .GetInterfaces()
                .Where(i => i.IsGenericType && i.GetGenericTypeDefinition() == handlerInterface);

            foreach (var serviceType in serviceTypes)
            {
                var eventType = serviceType.GetGenericArguments()[0];
                store.Register(eventType, type);
                services.TryAddScoped(type);
            }
        }

        services.TryAddSingleton(store);
    }

    /// <summary>
    ///
    /// </summary>
    /// <param name="builder"></param>
    /// <param name="configure"></param>
    /// <returns></returns>
    public static MicroserviceFrameworkBuilder UseLocalEventPublisher(this MicroserviceFrameworkBuilder builder,
        Action<LocalEventOptions> configure = null)
    {
        if (configure != null)
        {
            builder.Services.Configure(configure);
        }
        else
        {
            builder.Services.Configure<LocalEventOptions>(options =>
            {
                options.EnableAuditing = false;
            });
        }

        builder.Services.AddLocalEventPublisher();
        return builder;
    }
}
