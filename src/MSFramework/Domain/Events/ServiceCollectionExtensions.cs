using System;
using System.Linq;
using System.Reflection;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace MSFramework.Domain.Events
{
	public static class ServiceCollectionExtensions
	{
		public static IServiceCollection AddEventDispatcher(this IServiceCollection serviceCollection,
			params Type[] types)
		{
			var assemblies = types.Select(x => x.Assembly).ToArray();
			serviceCollection.AddEventDispatcher(assemblies);
			return serviceCollection;
		}

		public static IServiceCollection AddEventDispatcher(this IServiceCollection serviceCollection,
			params Assembly[] assemblies)
		{
			serviceCollection.TryAddScoped<IHandlerFactory, DependencyInjectionHandlerFactory>();
			serviceCollection.TryAddScoped<IEventDispatcher, EventDispatcher>();
			serviceCollection.RegisterEventHandler(assemblies);
			return serviceCollection;
		}

		private static void RegisterEventHandler(this IServiceCollection serviceCollection,
			params Assembly[] assemblies)
		{
			var store = new EventHandlerTypeStore();
			var types = assemblies.SelectMany(x => x.GetTypes());
			var handlerInterfaceType = typeof(IEventHandler);
			foreach (var handlerType in types)
			{
				if (handlerInterfaceType.IsAssignableFrom(handlerType))
				{
					var eventType = handlerType.GetInterface("IEventHandler`1")?.GenericTypeArguments
						.SingleOrDefault();
					if (eventType != null)
					{
						serviceCollection.TryAddScoped(handlerType);
						store.Add(eventType, handlerType);
					}
				}
			}

			serviceCollection.TryAddSingleton<IEventHandlerTypeStore>(store);
		}
	}
}