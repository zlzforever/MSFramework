using System;
using Microsoft.Extensions.DependencyInjection;

namespace MicroserviceFramework.Extensions.DependencyInjection;

/// <summary>
///
/// </summary>
public static class LifetimeUtilities
{
    /// <summary>
    ///
    /// </summary>
    /// <param name="type"></param>
    /// <returns></returns>
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
