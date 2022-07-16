using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using MicroserviceFramework.Collections.Generic;

namespace MicroserviceFramework.EventBus
{
	/// <summary>
	/// 相同的 EventName 可以对应不同的 EventType
	/// 所以需要通过 EventName 找到多个 EventType, 找到 IEventHandler, 再通过 DI 创建出所有注册对象
	/// </summary>
	public static class EventSubscriptionManager
	{
		/// <summary>
		/// KEY: 事件名
		/// Value->Key: 事件处理器的类型
		/// Value->Value: 事件处理器的方法
		/// </summary>
		private static readonly
			ConcurrentDictionary<string, ConcurrentDictionary<Type, SubscriptionInfo>> Cache;

		private static readonly ConcurrentDictionary<Type, object> EventTypes;

		static EventSubscriptionManager()
		{
			Cache = new ConcurrentDictionary<string, ConcurrentDictionary<Type, SubscriptionInfo>>();
			EventTypes = new ConcurrentDictionary<Type, object>();
		}

		public static bool IsEmpty => EventTypes.IsNullOrEmpty();

		public static bool Register(Type eventType, Type handlerType)
		{
			if (!eventType.IsEvent())
			{
				throw new ArgumentException($"{eventType} is not event type");
			}

			// TODO: verify handlerType & eventType

			EventTypes.TryAdd(eventType, null);

			var name = eventType.GetEventName();

			ConcurrentDictionary<Type, SubscriptionInfo> dict;
			if (!Cache.ContainsKey(name))
			{
				dict = new ConcurrentDictionary<Type, SubscriptionInfo>();
				Cache.TryAdd(name, dict);
			}
			else
			{
				dict = Cache[name];
			}

			return dict.TryAdd(handlerType, new SubscriptionInfo(eventType, handlerType));
		}

		public static IEnumerable<SubscriptionInfo> GetOrDefault(string eventName)
		{
			var dict = Cache.GetOrDefault(eventName);
			return dict?.Values;
		}

		public static IEnumerable<Type> GetEventTypes()
		{
			return EventTypes.Keys;
		}
	}
}