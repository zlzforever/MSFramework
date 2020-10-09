using System;
using System.Linq;

namespace MicroserviceFramework.Domain.Event
{
	public static class Extensions
	{
		public static bool CanHandle(this Type handlerType, Type eventType)
		{
			return handlerType.GetGenericArguments().FirstOrDefault() == eventType;
		}

		public static bool IsEvent(this Type eventType)
		{
			return typeof(DomainEvent).IsAssignableFrom(eventType);
		}
	}
}