using System;
using System.Linq;
using System.Threading.Tasks;
using MicroserviceFramework.Initializer;
using Microsoft.Extensions.DependencyInjection;

namespace MicroserviceFramework.EventBus
{
	public class EventHandlerRegister : Initializer.InitializerBase, INotAutomaticRegisterInitializer
	{
		private static readonly Type EventHandlerBaseType = typeof(IEventHandler<>);

		public override async Task InitializeAsync(IServiceProvider serviceProvider)
		{
			var eventBus = serviceProvider.GetRequiredService<IEventBus>();

			var assemblies =  AppDomain.CurrentDomain.GetAssemblies();
			var types = assemblies.SelectMany(x => x.GetTypes());

			foreach (var type in types)
			{
				var interfaces = type.GetInterfaces();
				var handlerInterfaceTypes = interfaces
					.Where(@interface => @interface.IsGenericType &&
					                     EventHandlerBaseType == @interface.GetGenericTypeDefinition())
					.ToList();

				if (handlerInterfaceTypes.Count == 0)
				{
					continue;
				}

				// 约束：一个事件处理器只能处理一个事件
				if (handlerInterfaceTypes.Count > 1)
				{
					throw new EventBusException($"{type.FullName} should impl one handler");
				}

				var handlerInterfaceType = handlerInterfaceTypes[0];
				var eventType = handlerInterfaceType.GenericTypeArguments[0];

				await eventBus.SubscribeAsync(eventType, type);
			}
		}
	}
}