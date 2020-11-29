using System;
using System.Collections.Generic;
using System.Reflection;
using MicroserviceFramework.Extensions;

namespace MicroserviceFramework.EventBus
{
	/// <summary>
	/// 相同的 EventName 可以对应不同的 EventType
	/// 所以需要通过 EventName 找到多个 EventType, 找到 IEventHandler<EventType>, 再通过 DI 创建出所有注册对象
	/// </summary>
	public class EventHandlerTypeCache
	{
		private static readonly Dictionary<string, HashSet<HandlerInfo>> Cache;
		private static readonly HashSet<Type> EventTypes;

		static EventHandlerTypeCache()
		{
			Cache = new Dictionary<string, HashSet<HandlerInfo>>();
			EventTypes = new HashSet<Type>();
		}

		public static void Register(Type eventType, Type handlerType, MethodInfo handlerMethod)
		{
			lock (Cache)
			{
				EventTypes.Add(eventType);
				var name = eventType.Name;
				if (!Cache.ContainsKey(name))
				{
					Cache[name] = new HashSet<HandlerInfo>();
				}

				Cache[name].Add(new HandlerInfo(eventType, handlerType, handlerMethod));
			}
		}

		public static IReadOnlyCollection<HandlerInfo> GetOrDefault(string eventName)
		{
			lock (Cache)
			{
				return Cache.GetOrDefault(eventName);
			}
		}

		public static IReadOnlyCollection<Type> GetEventTypes()
		{
			return EventTypes;
		}
	}
}