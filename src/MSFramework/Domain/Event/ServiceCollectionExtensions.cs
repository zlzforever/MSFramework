using System.Linq;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace MicroserviceFramework.Domain.Event
{
	public static class ServiceCollectionExtensions
	{
		public static IServiceCollection AddDomainEventDispatcher(this IServiceCollection serviceCollection)
		{
			serviceCollection.TryAddScoped<IDomainEventDispatcher, DomainEventDispatcher>();

			MicroserviceFrameworkLoader.RegisterType += type =>
			{
				if (type.IsAbstract || type.IsInterface)
				{
					return;
				}

				var interfaces = type.GetInterfaces();
				var handlerInterfaceTypes = interfaces
					.Where(@interface => @interface.IsGenericType && DomainEventDispatcher.EventHandlerBaseType ==
						@interface.GetGenericTypeDefinition())
					.ToList();

				if (handlerInterfaceTypes.Count == 0)
				{
					return;
				}

				foreach (var handlerInterfaceType in handlerInterfaceTypes)
				{
					var eventType = handlerInterfaceType.GenericTypeArguments[0];
					var handlerMethod = handlerInterfaceType.GetMethod("HandleAsync", new[] {eventType});
					DomainEventDispatcher.Register(eventType, (handlerInterfaceType, handlerMethod));
					serviceCollection.AddScoped(handlerInterfaceType, type);
				}
			};
			return serviceCollection;
		}
	}
}