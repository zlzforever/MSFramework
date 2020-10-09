using System;
using System.Collections.Concurrent;
using System.Collections.Generic;

namespace MicroserviceFramework.Domain.Event
{
	public class EventHandlerTypeStore : IEventHandlerTypeStore
	{
		private readonly ConcurrentDictionary<Type, HashSet<HandlerInfo>> _eventHandlerTypesDict;
		private static readonly IReadOnlyCollection<HandlerInfo> Empty = new List<HandlerInfo>();

		public EventHandlerTypeStore()
		{
			_eventHandlerTypesDict = new ConcurrentDictionary<Type, HashSet<HandlerInfo>>();
		}

		public void Add(Type eventType, Type handlerType)
		{
			var methodInfo = handlerType.GetMethod("HandleAsync");
			if (methodInfo == null)
			{
				throw new MicroserviceFrameworkException($"在类型 {handlerType.FullName} 中找不到处理方法 HandleAsync");
			}

			if (!_eventHandlerTypesDict.ContainsKey(eventType))
			{
				_eventHandlerTypesDict.TryAdd(eventType, new HashSet<HandlerInfo>());
			}

			_eventHandlerTypesDict[eventType].Add(new HandlerInfo(handlerType, methodInfo));
		}

		public IReadOnlyCollection<HandlerInfo> GetHandlers(Type eventType)
		{
			if (_eventHandlerTypesDict.TryGetValue(eventType, out var handlerTypes))
			{
				return handlerTypes;
			}

			return Empty;
		}
	}
}