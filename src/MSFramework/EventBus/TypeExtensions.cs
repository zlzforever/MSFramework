using System;
using System.Reflection;

namespace MicroserviceFramework.EventBus
{
	public static class TypeExtensions
	{
		public static bool IsEvent(this Type eventType)
		{
			return typeof(EventBase).IsAssignableFrom(eventType);
		}

		public static MethodInfo GetHandlerMethod(this Type handlerType, Type eventType)
		{
			var type = typeof(IEventHandler<>).MakeGenericType(eventType);
			return type.IsAssignableFrom(handlerType) ? type.GetMethod("HandleAsync", new[] {eventType}) : null;
		}
	}
}