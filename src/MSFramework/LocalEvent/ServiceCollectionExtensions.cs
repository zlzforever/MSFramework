using System;
using System.Linq;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace MicroserviceFramework.LocalEvent;

/// <summary>
/// 本地事件总线服务注册扩展方法
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
        services.TryAddSingleton<LocalEventChannel>();
        services.TryAddSingleton<LocalEventBackgroundService>();
        services.AddHostedService(serviceProvider =>
            serviceProvider.GetRequiredService<LocalEventBackgroundService>());

        var handlerInterface = typeof(IEventHandler<>);

        var store = new EventDescriptorStore();
        foreach (var type in Utils.Runtime.GetAllTypes())
        {
            // 抽象类、开放泛型不能注册：abstract class BaseHandler : IEventHandler<E>
            if (!type.IsClass || type.IsAbstract || type.IsGenericTypeDefinition)
            {
                continue;
            }

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

        store.Freeze();
        services.TryAddSingleton(store);
    }

    /// <summary>
    /// 使用本地事件发布器
    /// </summary>
    /// <param name="builder">框架构建器</param>
    /// <param name="configure">本地事件配置委托</param>
    /// <returns>框架构建器</returns>
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
