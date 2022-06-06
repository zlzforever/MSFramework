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

		public static MicroserviceFrameworkBuilder UseEventBus(this MicroserviceFrameworkBuilder builder)
		{
			builder.Services.TryAddSingleton<IEventBus, InProcessEventBus>();

			MicroserviceFrameworkLoaderContext.Get(builder.Services).ResolveType += type =>
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
					if (!eventType.IsEvent())
					{
						throw new MicroserviceFrameworkException($"{eventType} 不是合法的事件类型");
					}
					
					// 消息队列，得知道系统实现了哪些 EventHandler 才去监听对应的 Topic，所以必须先注册监听。
					EventHandlerTypeCache.Register(eventType, handlerInterfaceType);
					// 每次收到的消息都是独立的 Scope
					ServiceCollectionUtilities.TryAdd(builder.Services,
						new ServiceDescriptor(handlerInterfaceType, type, ServiceLifetime.Scoped));
				}
			};

			return builder;
		}
	}
}