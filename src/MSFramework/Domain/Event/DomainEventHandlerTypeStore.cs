using System;
using System.Collections.Concurrent;
using System.Collections.Generic;

namespace MicroserviceFramework.Domain.Event
{
	public class DomainEventHandlerTypeStore : IDomainEventHandlerTypeStore
	{
		private readonly ConcurrentDictionary<Type, HashSet<DomainEventHandlerInfo>> _eventHandlerTypesDict;
		private static readonly IReadOnlyCollection<DomainEventHandlerInfo> Empty = new List<DomainEventHandlerInfo>();

		public DomainEventHandlerTypeStore()
		{
			_eventHandlerTypesDict = new ConcurrentDictionary<Type, HashSet<DomainEventHandlerInfo>>();
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
				_eventHandlerTypesDict.TryAdd(eventType, new HashSet<DomainEventHandlerInfo>());
			}

			_eventHandlerTypesDict[eventType].Add(new DomainEventHandlerInfo(handlerType, methodInfo));
		}

		public IReadOnlyCollection<DomainEventHandlerInfo> GetHandlers(Type eventType)
		{
			if (_eventHandlerTypesDict.TryGetValue(eventType, out var handlerTypes))
			{
				return handlerTypes;
			}

			return Empty;
		}
	}
}