using System;
using System.Linq;

namespace MicroserviceFramework.EventBus
{
	public static class Extensions
	{
		public static bool IsIntegrationEvent(this Type eventType)
		{
			return typeof(Event).IsAssignableFrom(eventType);
		}

		public static bool CanHandle(this Type handlerType, Type eventType)
		{
			return handlerType.GetGenericArguments().FirstOrDefault() == eventType;
		}
	}
}