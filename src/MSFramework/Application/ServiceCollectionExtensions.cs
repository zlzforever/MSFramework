using System;
using System.Linq;
using System.Reflection;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace MSFramework.Application
{
	public static class ServiceCollectionExtensions
	{
		public static IServiceCollection AddRequestExecutor(this IServiceCollection serviceCollection,
			params Type[] types)
		{
			var assemblies = types.Select(x => x.Assembly).ToArray();
			serviceCollection.AddRequestExecutor(assemblies);
			return serviceCollection;
		}

		public static IServiceCollection AddRequestExecutor(this IServiceCollection serviceCollection,
			params Assembly[] assemblies)
		{
			var types = assemblies.SelectMany(x => x.GetTypes()).ToArray();
			var cache = new RequestHandlerTypeCache();
			var handlerInterfaceType = typeof(IRequestHandler);
			var handlerInterfaceName = handlerInterfaceType.FullName;
			if (string.IsNullOrWhiteSpace(handlerInterfaceName))
			{
				throw new ArgumentException(nameof(handlerInterfaceName));
			}

			foreach (var handlerType in types)
			{
				if (handlerType != handlerInterfaceType
				    && handlerInterfaceType.IsAssignableFrom(handlerType))
				{
					var interface1 = handlerType.GetInterfaces()
						.FirstOrDefault(x =>
							!string.IsNullOrWhiteSpace(x.FullName)
							&& x.FullName != handlerInterfaceName
							&& x.FullName.StartsWith(handlerInterfaceName)
						);
					if (interface1 == null)
					{
						continue;
					}

					var commandType = interface1.GenericTypeArguments
						.FirstOrDefault();
					if (commandType != null && !cache.ContainsKey(commandType))
					{
						serviceCollection.TryAddScoped(handlerType);
						cache.TryAdd(commandType, (handlerType, handlerType.GetMethod("HandleAsync")));
					}
				}
			}

			serviceCollection.TryAddSingleton(cache);
			serviceCollection.TryAddScoped<IRequestProcessor, DefaultRequestProcessor>();

			return serviceCollection;
		}
	}
}