using System;
using Microsoft.Extensions.DependencyInjection;

namespace MicroserviceFramework.Extensions.DependencyInjection;

/// <summary>
/// 依赖注入生命周期工具类，根据标记接口获取服务生命周期
/// </summary>
public static class LifetimeUtilities
{
    /// <summary>
    /// 根据类型实现的标记接口获取对应的依赖注入生命周期
    /// </summary>
    /// <param name="type">要检查的类型</param>
    /// <returns>服务生命周期，若不实现任何标记接口则返回 null</returns>
    public static ServiceLifetime? GetLifetime(Type type)
    {
        if (type.IsAbstract || type.IsInterface)
        {
            return null;
        }

        if (typeof(ISingletonDependency).IsAssignableFrom(type))
        {
            return ServiceLifetime.Singleton;
        }

        if (typeof(IScopeDependency).IsAssignableFrom(type))
        {
            return ServiceLifetime.Scoped;
        }

        if (typeof(ITransientDependency).IsAssignableFrom(type))
        {
            return ServiceLifetime.Transient;
        }

        return null;
    }
}
