using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace MicroserviceFramework.Domain.Events
{
	public class EventHandlerTypeStore : IEventHandlerTypeStore
	{
		private readonly ConcurrentDictionary<Type, ConcurrentDictionary<Type, MethodInfo>> _eventHandlerTypesDict;
		private readonly Dictionary<Type, MethodInfo> _empty = new Dictionary<Type, MethodInfo>();

		public EventHandlerTypeStore()
		{
			_eventHandlerTypesDict = new ConcurrentDictionary<Type, ConcurrentDictionary<Type, MethodInfo>>();
		}

		public bool Add(Type eventType, Type handlerType)
		{
			var methodInfo = handlerType.GetMethod("HandleAsync");
			if (methodInfo == null)
			{
				throw new MicroserviceFrameworkException($"在类型 {handlerType.FullName} 中找不到处理方法 HandleAsync");
			}

			if (!_eventHandlerTypesDict.ContainsKey(eventType) &&
			    !_eventHandlerTypesDict.TryAdd(eventType, new ConcurrentDictionary<Type, MethodInfo>()))
			{
				return false;
			}

			return _eventHandlerTypesDict[eventType].ContainsKey(handlerType) ||
			       _eventHandlerTypesDict[eventType].TryAdd(handlerType, methodInfo);
		}

		public IReadOnlyDictionary<Type, MethodInfo> GetHandlerTypes(Type eventType)
		{
			if (_eventHandlerTypesDict.TryGetValue(eventType, out var handlerTypes))
			{
				return handlerTypes;
			}

			return _empty;
		}
	}
}