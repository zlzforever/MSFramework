using System;
using System.Linq;
using System.Reflection;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace MSFramework.Application
{
	public static class ServiceCollectionExtensions
	{
		public static IServiceCollection AddCommandExecutor(this IServiceCollection serviceCollection,
			params Type[] types)
		{
			var assemblies = types.Select(x => x.Assembly).ToArray();
			serviceCollection.AddCommandExecutor(assemblies);
			return serviceCollection;
		}

		public static IServiceCollection AddCommandExecutor(this IServiceCollection serviceCollection,
			params Assembly[] assemblies)
		{
			var types = assemblies.SelectMany(x => x.GetTypes()).ToArray();
			var cache = new CommandHandlerTypeCache();
			var handlerInterfaceType = typeof(ICommandHandler);
			foreach (var handlerType in types)
			{
				if (handlerType != handlerInterfaceType
				    && handlerInterfaceType.IsAssignableFrom(handlerType))
				{
					var interface1 = handlerType.GetInterfaces()
						.FirstOrDefault(x =>
							!string.IsNullOrWhiteSpace(x.FullName)
							&& x.FullName != "MSFramework.Application.ICommandHandler"
							&& x.FullName.StartsWith("MSFramework.Application.ICommandHandler")
						);
					if (interface1 == null)
					{
						continue;
					}

					var commandType = interface1.GenericTypeArguments
						.FirstOrDefault();
					if (commandType != null && !cache.Contains(commandType))
					{
						serviceCollection.TryAddScoped(handlerType);
						cache.TryAdd(commandType, handlerType);
					}
				}
			}

			serviceCollection.TryAddSingleton(cache);
			serviceCollection.TryAddSingleton<ICommandExecutor, DefaultCommandExecutor>();

			return serviceCollection;
		}
	}
}