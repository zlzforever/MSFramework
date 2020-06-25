using System;
using System.Linq;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace MSFramework.Domain.Event
{
	public static class ServiceCollectionExtensions
	{
		public static IServiceCollection AddEventDispatcher(this IServiceCollection serviceCollection,
			params Type[] types)
		{
			serviceCollection.TryAddScoped<IHandlerFactory, DependencyInjectionHandlerFactory>();
			serviceCollection.TryAddScoped<IEventDispatcher, EventDispatcher>();
			serviceCollection.RegisterEventHandler(types);
			return serviceCollection;
		}

		private static IServiceCollection RegisterEventHandler(this IServiceCollection serviceCollection,
			params Type[] handlerTypes)
		{
			var store = new EventHandlerTypeStore();
			var types = handlerTypes.SelectMany(x => x.Assembly.GetTypes());

			foreach (var handlerType in types)
			{
				if (handlerType.IsHandler())
				{
					serviceCollection.TryAddScoped(handlerType);
				}

				var eventType = handlerType.GetInterface("IEventHandler`1")?.GenericTypeArguments
					.SingleOrDefault();
				if (eventType != null)
				{
					store.Add(eventType, handlerType);
				}
			}

			serviceCollection.TryAddSingleton<IEventHandlerTypeStore>(store);
			return serviceCollection;
		}
	}
}