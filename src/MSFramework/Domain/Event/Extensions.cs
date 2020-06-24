using System;
using System.Linq;

namespace MSFramework.Domain.Event
{
	public static class Extensions
	{
		public static bool IsHandler(this Type handlerType)
		{
			var eventType = handlerType.GetInterface("IEventHandler`1")?.GenericTypeArguments
				.SingleOrDefault();
			return eventType != null && eventType.IsEvent();
		}

		public static bool IsEvent(this Type eventType)
		{
			return typeof(EventBase).IsAssignableFrom(eventType);
		}
	}
}