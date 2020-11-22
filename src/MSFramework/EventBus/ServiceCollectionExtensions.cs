using System;
using System.Linq;
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

			MicroserviceFrameworkLoader.RegisterType += type =>
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
					serviceCollection.AddScoped(handlerInterfaceType, type);
				}
			};

			return serviceCollection;
		}
	}
}