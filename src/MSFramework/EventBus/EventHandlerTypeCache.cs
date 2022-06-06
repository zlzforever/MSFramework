using System;
using System.Collections.Concurrent;
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
		/// <summary>
		/// KEY: 事件名
		/// Value->Key: 事件处理器的类型
		/// Value->Value: 事件处理器的方法
		/// </summary>
		private static readonly
			ConcurrentDictionary<string, ConcurrentDictionary<Type, (Type EventType, MethodInfo MethodInfo)>> Cache;

		private static readonly ConcurrentDictionary<Type, object> EventTypes;

		static EventHandlerTypeCache()
		{
			Cache =
				new ConcurrentDictionary<string, ConcurrentDictionary<Type, (Type EventType, MethodInfo MethodInfo)>>();
			EventTypes = new ConcurrentDictionary<Type, object>();
		}

		public static void Register(Type eventType, Type handlerType)
		{
			var handlerMethod = handlerType.GetMethod("HandleAsync", new[] { eventType });

			EventTypes.TryAdd(eventType, null);
			var name = eventType.FullName;
			Cache.AddOrUpdate(name, _ =>
			{
				var dict = new ConcurrentDictionary<Type, (Type EventType, MethodInfo MethodInfo)>();
				dict.TryAdd(handlerType, (eventType, handlerMethod));
				return dict;
			}, (_, x) =>
			{
				x.TryAdd(handlerType, (eventType, handlerMethod));
				return x;
			});
		}

		public static ConcurrentDictionary<Type, (Type EventType, MethodInfo MethodInfo)> GetOrDefault(string eventName)
		{
			return Cache.GetOrDefault(eventName);
		}

		public static ICollection<Type> GetEventTypes()
		{
			return EventTypes.Keys;
		}
	}
}