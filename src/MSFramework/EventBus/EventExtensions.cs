using System;
using System.Collections.Concurrent;
using System.Reflection;

namespace MicroserviceFramework.EventBus
{
	public static class EventExtensions
	{
		private static readonly ConcurrentDictionary<Type, bool> EventTypes;

		static EventExtensions()
		{
			EventTypes = new ConcurrentDictionary<Type, bool>();
		}

		public static bool IsEvent(this Type eventType)
		{
			return EventTypes.GetOrAdd(eventType, type =>
			{
				if (typeof(EventBase).IsAssignableFrom(type))
				{
					return true;
				}

				return type.GetCustomAttribute<EventAttribute>() != null &&
				       type.GetProperty("EventId") != null &&
				       type.GetProperty("EventTime") != null;
			});
		}

		public static MethodInfo GetHandlerMethod(this Type handlerType, Type eventType)
		{
			var type = typeof(IEventHandler<>).MakeGenericType(eventType);
			return type.IsAssignableFrom(handlerType) ? type.GetMethod("HandleAsync", new[] {eventType}) : null;
		}
	}
}