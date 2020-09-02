using System;
using System.Linq;
using System.Reflection;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace MicroserviceFramework.Domain.Events
{
	public static class ServiceCollectionExtensions
	{
		private static readonly Type EventHandlerBaseType = typeof(IEventHandler<>);

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
			serviceCollection.TryAddSingleton<IEventDispatcher, EventDispatcher>();
			serviceCollection.RegisterEventHandler(assemblies);
			return serviceCollection;
		}

		private static void RegisterEventHandler(this IServiceCollection serviceCollection,
			params Assembly[] assemblies)
		{
			var store = new EventHandlerTypeStore();
			var types = assemblies.SelectMany(x => x.GetTypes());

			foreach (var type in types)
			{
				var interfaces = type.GetInterfaces();
				if (interfaces.Any(x => x.IsGenericType && x.GetGenericTypeDefinition() == EventHandlerBaseType))
				{
					var eventType = type.GetInterface("IEventHandler`1")?.GenericTypeArguments
						.SingleOrDefault();
					if (eventType != null)
					{
						serviceCollection.TryAddScoped(type);
						store.Add(eventType, type);
					}
				}
			}

			serviceCollection.TryAddSingleton<IEventHandlerTypeStore>(store);
		}
	}
}