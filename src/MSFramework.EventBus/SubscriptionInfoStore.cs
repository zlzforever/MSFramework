using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;

namespace MicroserviceFramework.EventBus
{
	public class SubscriptionInfoStore : ISubscriptionInfoStore
	{
		private readonly ConcurrentDictionary<string, HashSet<SubscriptionInfo>> _eventHandlerTypesDict;
		private static readonly IReadOnlyCollection<SubscriptionInfo> Empty = new List<SubscriptionInfo>();
		private readonly HashSet<Type> _eventTypes;

		public SubscriptionInfoStore()
		{
			_eventTypes = new HashSet<Type>();
			_eventHandlerTypesDict = new ConcurrentDictionary<string, HashSet<SubscriptionInfo>>();
		}

		public void Add<TEvent, TEventHandler>() where TEvent : Event
			where TEventHandler : IEventHandler<TEvent>
		{
			Add(typeof(TEvent), typeof(TEventHandler));
		}

		public void Add(Type eventType, Type handlerType)
		{
			if (!eventType.IsEvent())
			{
				throw new MicroserviceFrameworkException($"{eventType} 不是一个集成事件");
			}

			if (!handlerType.CanHandle(eventType))
			{
				throw new MicroserviceFrameworkException($"{handlerType} 不能处理 {eventType}");
			}

			var methodInfo = handlerType.GetMethod("HandleAsync");
			if (methodInfo == null)
			{
				throw new MicroserviceFrameworkException($"在类型 {handlerType.FullName} 中找不到处理方法 HandleAsync");
			}

			var eventName = GetEventKey(eventType);
			if (!_eventHandlerTypesDict.ContainsKey(eventName))
			{
				_eventHandlerTypesDict.TryAdd(eventName, new HashSet<SubscriptionInfo>());
			}

			_eventHandlerTypesDict[eventName].Add(new SubscriptionInfo(eventType, handlerType, methodInfo));
			_eventTypes.Add(eventType);
		}

		public IReadOnlyCollection<SubscriptionInfo> GetHandlers(string eventType)
		{
			if (_eventHandlerTypesDict.TryGetValue(eventType, out var handlerTypes))
			{
				return handlerTypes;
			}

			return Empty;
		}

		public string GetEventKey<T>()
		{
			return GetEventKey(typeof(T));
		}

		public string GetEventKey(Type type)
		{
			return type.Name;
		}

		public void Remove<TEvent, TEventHandler>() where TEvent : Event where TEventHandler : IEventHandler<TEvent>
		{
			var eventName = GetEventKey<TEvent>();
			var eventHandlerType = typeof(TEventHandler);
			if (_eventHandlerTypesDict.TryGetValue(eventName, out var handlerTypes))
			{
				var handlerType = handlerTypes
					.SingleOrDefault(x => x.HandlerType == eventHandlerType);
				if (handlerType != null)
				{
					handlerTypes.Remove(handlerType);
				}
			}

			_eventTypes.Remove(typeof(TEvent));
		}
	}
}