using System;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using MicroserviceFramework.Utilities;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace MicroserviceFramework.Initializer
{
	public static class ServiceCollectionExtensions
	{
		internal static IServiceCollection AddInitializer(this IServiceCollection services)
		{
			var initializerType = typeof(InitializerBase);
			MicroserviceFrameworkLoaderContext.Default.ResolveType += type =>
			{
				if (type.IsAbstract || type.IsInterface)
				{
					return;
				}

				if (initializerType.IsAssignableFrom(type) &&
				    type.GetCustomAttribute(typeof(NotRegisterAttribute)) == null)
				{
					ServiceCollectionUtilities.TryAdd(services,
						new ServiceDescriptor(initializerType, type, ServiceLifetime.Singleton));
				}
			};
			return services;
		}

		public static MicroserviceFrameworkBuilder AddInitializer<TInitializer>(
			this MicroserviceFrameworkBuilder builder)
			where TInitializer : InitializerBase
		{
			builder.Services.AddSingleton<TInitializer>();
			return builder;
		}

		public static IServiceCollection AddInitializer<TInitializer>(this IServiceCollection serviceCollection)
			where TInitializer : InitializerBase
		{
			serviceCollection.AddSingleton<InitializerBase, TInitializer>();
			return serviceCollection;
		}

		public static async Task UseInitializerAsync(this IServiceProvider applicationServices)
		{
			using var scope = applicationServices.CreateScope();
			var logger = scope.ServiceProvider.GetRequiredService<ILoggerFactory>().CreateLogger("Initializer");
			var initializers = scope.ServiceProvider.GetServices<InitializerBase>().OrderBy(x => x.Order)
				.ToList();
			logger.LogInformation($"{string.Join(" -> ", initializers.Select(x => x.GetType().FullName))}");
			foreach (var initializer in initializers)
			{
				await initializer.InitializeAsync(scope.ServiceProvider);
			}
		}
	}
}