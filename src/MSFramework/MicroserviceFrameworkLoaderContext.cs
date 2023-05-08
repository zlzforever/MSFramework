using System;
using System.Collections.Concurrent;
using Microsoft.Extensions.DependencyInjection;

namespace MicroserviceFramework;

public class MicroserviceFrameworkLoaderContext
{
    private static readonly ConcurrentDictionary<IServiceCollection, MicroserviceFrameworkLoaderContext> Contexts =
        new();

    private MicroserviceFrameworkLoaderContext()
    {
    }

    public event Action<Type> ResolveType;

    public static MicroserviceFrameworkLoaderContext Get(IServiceCollection serviceCollection)
    {
        return Contexts.GetOrAdd(serviceCollection, _ => new MicroserviceFrameworkLoaderContext());
    }

    public void LoadTypes()
    {
        if (ResolveType == null)
        {
            return;
        }

        var types = Utils.Runtime.GetAllTypes();

        foreach (var type in types)
        {
            ResolveType(type);
        }
    }
}
