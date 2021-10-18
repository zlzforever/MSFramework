using System;
using System.Linq;
using MicroserviceFramework.Application.CQRS;
using MicroserviceFramework.DependencyInjection;
using MicroserviceFramework.Utilities;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace MicroserviceFramework.Application
{
	public static class ServiceCollectionExtensions
	{
		private static void RegisterGenericType(IServiceCollection serviceCollection, Type type,
			Type genericTypeDefinition)
		{
			if (type.IsAbstract || type.IsInterface)
			{
				return;
			}

			var handlerInterfaceTypes = type.GetInterfaces()
				.Where(@interface => @interface.IsGenericType && genericTypeDefinition ==
					@interface.GetGenericTypeDefinition())
				.ToList();

			if (handlerInterfaceTypes.Count == 0)
			{
				return;
			}
			
			foreach (var handlerInterfaceType in handlerInterfaceTypes)
			{
				var lifetime = LifetimeUtilities.GetLifetime(type);
				ServiceCollectionUtilities.TryAdd(serviceCollection,
					new ServiceDescriptor(handlerInterfaceType, type,
						lifetime ?? ServiceLifetime.Scoped));
			}
		}
	}
}