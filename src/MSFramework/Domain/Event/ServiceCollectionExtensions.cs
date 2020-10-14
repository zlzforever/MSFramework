using System;
using System.Linq;
using System.Reflection;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace MicroserviceFramework.Domain.Event
{
	public static class ServiceCollectionExtensions
	{
		private static readonly Type EventHandlerBaseType = typeof(IDomainEventHandler<>);

		public static IServiceCollection AddDomainEventDispatcher(this IServiceCollection serviceCollection,
			params Type[] types)
		{
			var assemblies = types.Select(x => x.Assembly).ToArray();
			serviceCollection.AddDomainEventDispatcher(assemblies);
			return serviceCollection;
		}

		public static IServiceCollection AddDomainEventDispatcher(this IServiceCollection serviceCollection,
			params Assembly[] assemblies)
		{
			serviceCollection.TryAddScoped<IDomainEventDispatcher, DomainEventDispatcher>();
			serviceCollection.TryAddScoped<IDomainHandlerFactory, DependencyInjectionHandlerFactory>();
			serviceCollection.RegisterEventHandler(assemblies);
			return serviceCollection;
		}

		private static void RegisterEventHandler(this IServiceCollection serviceCollection,
			params Assembly[] assemblies)
		{
			var store = new DomainEventHandlerTypeStore();
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

				// 约束：一个事件处理器只能处理一个事件
				if (handlerTypes.Count > 1)
				{
					throw new MicroserviceFrameworkException($"{type.FullName} should impl one handler");
				}

				var handlerType = handlerTypes[0];
				var eventType = handlerType.GenericTypeArguments[0];

				serviceCollection.TryAddScoped(type);
				store.Add(eventType, type);
			}

			serviceCollection.TryAddSingleton<IDomainEventHandlerTypeStore>(store);
		}
	}
}