
using Microsoft.Extensions.DependencyInjection;

namespace MicroserviceFramework.Ef.Design;

/// <summary>
///
/// </summary>
public static class ServiceCollectionExtensions
{
    /// <summary>
    /// 
    /// </summary>
    /// <param name="serviceCollection"></param>
    /// <returns></returns>
    public static IServiceCollection ClearForeignKeys(this IServiceCollection serviceCollection)
    {
        // serviceCollection.RemoveAll<ICSharpMigrationOperationGenerator>();
        // serviceCollection
        // 	.AddSingleton<ICSharpMigrationOperationGenerator, ClearForeignKeysCSharpMigrationOperationGenerator>();
        return serviceCollection;
    }
}
