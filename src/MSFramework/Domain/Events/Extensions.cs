using System;
using System.Linq;

namespace MicroserviceFramework.Domain.Events
{
	public static class Extensions
	{
		public static bool CanHandle(this Type handlerType, Type eventType)
		{
			var eventType1 = handlerType.GetInterface("IEventHandler`1")?.GenericTypeArguments
				.SingleOrDefault();
			return eventType1 != null && eventType1.IsEvent() && eventType == eventType1;
		}

		public static bool IsEvent(this Type eventType)
		{
			return typeof(Event).IsAssignableFrom(eventType);
		}
	}
}