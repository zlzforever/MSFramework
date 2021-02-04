using System;
using System.Linq;
using MicroserviceFramework.Utilities;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace MicroserviceFramework.EventBus
{
	public static class ServiceCollectionExtensions
	{
		private static readonly Type EventHandlerBaseType = typeof(IEventHandler<>);

		public static IServiceCollection AddEventBus(this IServiceCollection serviceCollection)
		{
			serviceCollection.TryAddSingleton<IEventBus, InProcessEventBus>();
			serviceCollection
				.TryAddSingleton<IEventHandlerFactory, DependencyInjectionEventHandlerFactory>();

			MicroserviceFrameworkLoaderContext.Default.ResolveType += type =>
			{
				var interfaces = type.GetInterfaces();
				var handlerInterfaceTypes = interfaces
					.Where(@interface => @interface.IsGenericType &&
					                     EventHandlerBaseType == @interface.GetGenericTypeDefinition())
					.ToList();

				if (handlerInterfaceTypes.Count == 0)
				{
					return;
				}

				foreach (var handlerInterfaceType in handlerInterfaceTypes)
				{
					var eventType = handlerInterfaceType.GenericTypeArguments[0];
					var handlerMethod = handlerInterfaceType.GetMethod("HandleAsync", new[] {eventType});
					EventHandlerTypeCache.Register(eventType, handlerInterfaceType, handlerMethod);
					ServiceCollectionUtilities.TryAdd(serviceCollection,
						new ServiceDescriptor(handlerInterfaceType, type, ServiceLifetime.Scoped));
				}
			};

			return serviceCollection;
		}
	}
}