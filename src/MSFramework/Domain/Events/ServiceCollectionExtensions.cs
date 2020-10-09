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
			serviceCollection.TryAddScoped<IHandlerFactory, DependencyInjectionHandlerFactory>();
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
				var handlerTypes = interfaces
					.Where(@interface => @interface.IsGenericType)
					.Where(@interface => EventHandlerBaseType == @interface.GetGenericTypeDefinition())
					.ToList();

				if (handlerTypes.Count == 0)
				{
					continue;
				}

				if (handlerTypes.Count > 1)
				{
					throw new MicroserviceFrameworkException($"{type.FullName} should impl one handler");
				}

				var handlerType = handlerTypes.First();
				var eventType = handlerType.GenericTypeArguments.First();

				serviceCollection.TryAddScoped(type);
				store.Add(eventType.Name, type);
			}

			serviceCollection.TryAddSingleton<IEventHandlerTypeStore>(store);
		}
	}
}