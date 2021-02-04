using MicroserviceFramework.Initializer;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace MicroserviceFramework.FeatureManagement
{
	public static class ServiceCollectionExtensions
	{
		public static IServiceCollection AddFunction<TFunctionDefineFinder>(this IServiceCollection serviceCollection)
			where TFunctionDefineFinder : class, IFeatureFinder
		{
			serviceCollection.AddInitializer<FeatureInitializer>();
			serviceCollection.TryAddSingleton<IFeatureFinder, TFunctionDefineFinder>();
			return serviceCollection;
		}
	}
}