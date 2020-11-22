using MicroserviceFramework.Initializer;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace MicroserviceFramework.Function
{
	public static class ServiceCollectionExtensions
	{
		public static IServiceCollection AddFunction<TFunctionDefineFinder>(this IServiceCollection serviceCollection)
			where TFunctionDefineFinder : class, IFunctionDefineFinder
		{
			serviceCollection.AddInitializer<FunctionInitializer>();
			serviceCollection.TryAddSingleton<IFunctionDefineFinder, TFunctionDefineFinder>();
			return serviceCollection;
		}
	}
}