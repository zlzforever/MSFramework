using System;
using System.Linq;
using System.Threading.Tasks;
using MicroserviceFramework.Initializers;
using MicroserviceFramework.Reflection;
using Microsoft.Extensions.DependencyInjection;

namespace MicroserviceFramework.EventBus
{
	public class EventBusInitializer : Initializer
	{
		private static readonly Type EventHandlerBaseType = typeof(IIntegrationEventHandler<>);

		public override async Task InitializeAsync(IServiceProvider serviceProvider)
		{
			var eventBus = serviceProvider.GetService<IEventBus>();
			if (eventBus == null)
			{
				return;
			}

			var assemblies = AssemblyFinder.GetAllList();
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

				await eventBus.SubscribeAsync(eventType, handlerType);
			}
		}
	}
}