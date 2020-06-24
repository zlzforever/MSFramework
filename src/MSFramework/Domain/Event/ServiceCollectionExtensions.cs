using System;
using System.Linq;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace MSFramework.Domain.Event
{
	public static class ServiceCollectionExtensions
	{
		public static IServiceCollection AddEventMediator(this IServiceCollection serviceCollection,
			params Type[] types)
		{
			serviceCollection.TryAddScoped<IHandlerFactory, DependencyInjectionHandlerFactory>();
			serviceCollection.TryAddScoped<IEventMediator, EventMediator>();
			serviceCollection.RegisterEvents(types);
			return serviceCollection;
		}

		private static IServiceCollection RegisterEvents(this IServiceCollection serviceCollection,
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

			serviceCollection.AddSingleton<IEventHandlerTypeStore>(store);
			return serviceCollection;
		}
	}
}