using System.Linq;
using MicroserviceFramework.Utilities;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace MicroserviceFramework.Domain.Event
{
	public static class ServiceCollectionExtensions
	{
		public static IServiceCollection AddDomainEvent(this IServiceCollection serviceCollection)
		{
			serviceCollection.TryAddScoped<IDomainEventDispatcher, DomainEventDispatcher>();

			MicroserviceFrameworkLoaderContext.Default.ResolveType += type =>
			{
				if (type.IsAbstract || type.IsInterface)
				{
					return;
				}

				var handlerInterfaceTypes = type.GetInterfaces()
					.Where(@interface => @interface.IsGenericType &&
					                     DomainEventDispatcher.EventHandlerBaseType ==
					                     @interface.GetGenericTypeDefinition())
					.ToList();

				if (handlerInterfaceTypes.Count == 0)
				{
					return;
				}

				foreach (var handlerInterfaceType in handlerInterfaceTypes)
				{
					ServiceCollectionUtilities.TryAdd(serviceCollection,
						new ServiceDescriptor(handlerInterfaceType, type, ServiceLifetime.Scoped));
				}
			};
			return serviceCollection;
		}
	}
}