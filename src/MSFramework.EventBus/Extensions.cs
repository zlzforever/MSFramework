using System;

namespace MicroserviceFramework.EventBus
{
	public static class Extensions
	{
		public static bool IsEvent(this Type eventType)
		{
			return typeof(Event).IsAssignableFrom(eventType);
		}

		public static bool CanHandle(this Type handlerType, Type eventType)
		{
			var type = typeof(IEventHandler<>).MakeGenericType(eventType);
			return type.IsAssignableFrom(handlerType);
		}
	}
}