namespace MicroserviceFramework;

/// <summary>
///
/// </summary>
public class MicroserviceFrameworkLoaderContext
{
    // private static readonly ConcurrentDictionary<IServiceCollection, MicroserviceFrameworkLoaderContext> Contexts =
    //     new();

    private MicroserviceFrameworkLoaderContext()
    {
    }

    // /// <summary>
    // ///
    // /// </summary>
    // public event Action<Type> ResolveType;

    // /// <summary>
    // ///
    // /// </summary>
    // /// <param name="serviceCollection"></param>
    // /// <returns></returns>
    // public static MicroserviceFrameworkLoaderContext Get(IServiceCollection serviceCollection)
    // {
    //     return Contexts.GetOrAdd(serviceCollection, _ => new MicroserviceFrameworkLoaderContext());
    // }

    // /// <summary>
    // ///
    // /// </summary>
    // public void LoadTypes()
    // {
    //     if (ResolveType == null)
    //     {
    //         return;
    //     }
    //
    //     var types = Utils.Runtime.GetAllTypes();
    //
    //     foreach (var type in types)
    //     {
    //         ResolveType(type);
    //     }
    // }
}
