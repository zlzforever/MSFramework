using System;
using System.Linq;
using MicroserviceFramework.Application;
using MicroserviceFramework.Domain;
using MicroserviceFramework.Mediator;
using MicroserviceFramework.Runtime;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace MicroserviceFramework.Extensions.DependencyInjection;

/// <summary>
///
/// </summary>
public static class ServiceCollectionExtensions
{
    /// <summary>
    /// 使用自动依赖注入组件，扫描程序集中的类型，对实现了
    /// IScopeDependency、ISingletonDependency、ITransientDependency
    /// 接口的类型， 自动注入到依赖注入中
    /// </summary>
    /// <param name="builder"></param>
    /// <returns></returns>
    public static MicroserviceFrameworkBuilder UseDependencyInjectionLoader(
        this MicroserviceFrameworkBuilder builder)
    {
        var types = Utils.Runtime.GetAllTypes();
        foreach (var type in types)
        {
            var lifetime = LifetimeUtilities.GetLifetime(type);
            if (lifetime.HasValue)
            {
                builder.Services.RegisterDependencyInjection(type, lifetime.Value);
            }
        }

        return builder;
    }

    /// <summary>
    /// 以类型实现的接口进行服务添加，需排除
    /// <see cref="IDisposable"/>等非业务接口，如无接口则注册自身
    /// </summary>
    /// <param name="services">服务映射信息集合</param>
    /// <param name="implementationType">要注册的实现类型集合</param>
    /// <param name="lifetime">注册的生命周期类型</param>
    private static void RegisterDependencyInjection(this IServiceCollection services,
        Type implementationType, ServiceLifetime lifetime)
    {
        if (implementationType.IsAbstract || implementationType.IsInterface)
        {
            return;
        }

        // 1. 注册类型本身
        // 同一类型只能有一种生命周期！！！想支持多种生命周期是：反设计、反最佳实践、高风险的行为。因此，实接使用原生接口注册即可。
        services.Add(new ServiceDescriptor(implementationType, implementationType, lifetime));

        var interfaceTypes = implementationType.GetInterfacesExcludeBy(
            typeof(ITransientDependency),
            typeof(ISingletonDependency),
            typeof(IScopeDependency),
            typeof(IDomainService),
            typeof(ISession),
            typeof(Request),
            typeof(Request<>)).ToArray();

        if (interfaceTypes.Length == 0)
        {
            return;
        }

        interfaceTypes = interfaceTypes.Where(interfaceType => !(interfaceType.IsGenericType &&
                                                                 interfaceType.GetGenericTypeDefinition() ==
                                                                 typeof(IDomainEventHandler<>))).ToArray();
        for (var i = 0; i < interfaceTypes.Length; i++)
        {
            var interfaceType = interfaceTypes[i];

            // IDomainEventHandler 继承自 IRequestHandler，不需要重复注册
            if (interfaceType.IsGenericType &&
                interfaceType.GetGenericTypeDefinition() == typeof(IDomainEventHandler<>))
            {
                continue;
            }

            // 瞬时生命周期每次获取对象都是新的，因此实现的接口每个注册一次即可
            if (lifetime == ServiceLifetime.Transient)
            {
                services.Add(
                    new ServiceDescriptor(interfaceType, implementationType, ServiceLifetime.Transient));
            }
            else
            {
                if (i == 0)
                {
                    services.Add(
                        new ServiceDescriptor(interfaceType, implementationType, lifetime));
                }
                else
                {
                    services.Add(new ServiceDescriptor(interfaceType,
                        provider => provider.GetService(interfaceTypes[0]), lifetime));
                }
            }
        }
    }
}
