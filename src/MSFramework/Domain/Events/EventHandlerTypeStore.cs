using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;

namespace MicroserviceFramework.Domain.Events
{
	public class EventHandlerTypeStore : IEventHandlerTypeStore
	{
		private readonly ConcurrentDictionary<Type, ConcurrentDictionary<Type, object>> _eventHandlerTypesDict;

		public EventHandlerTypeStore()
		{
			_eventHandlerTypesDict = new ConcurrentDictionary<Type, ConcurrentDictionary<Type, object>>();
		}

		public bool Add(Type eventType, Type handlerType)
		{
			if (!_eventHandlerTypesDict.ContainsKey(eventType))
			{
				if (!_eventHandlerTypesDict.TryAdd(eventType, new ConcurrentDictionary<Type, object>()))
				{
					return false;
				}
			}

			return _eventHandlerTypesDict[eventType].ContainsKey(handlerType) ||
			       _eventHandlerTypesDict[eventType].TryAdd(handlerType, null);
		}

		public IEnumerable<Type> GetHandlerTypes(Type eventType)
		{
			if (_eventHandlerTypesDict.TryGetValue(eventType, out var handlerTypes))
			{
				return handlerTypes.Keys;
			}

			return Enumerable.Empty<Type>();
		}
	}
}