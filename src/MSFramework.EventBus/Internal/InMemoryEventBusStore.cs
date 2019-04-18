using System;
using System.Collections.Generic;
using System.Linq;

namespace MSFramework.EventBus.Internal
{
	public class InMemoryEventBusSubscriptionStore : IEventBusSubscriptionStore
	{
		private readonly Dictionary<string, List<SubscriptionInfo>> _handlers;
		private readonly List<Type> _eventTypes;

		public InMemoryEventBusSubscriptionStore()
		{
			_handlers = new Dictionary<string, List<SubscriptionInfo>>();
			_eventTypes = new List<Type>();
		}

		public bool IsEmpty => !_handlers.Keys.Any();

		public void Clear() => _handlers.Clear();

		public event EventHandler<string> OnEventRemoved;

		public void AddSubscription<TH>(string eventName) where TH : IDynamicEventHandler
		{
			AddSubscription(typeof(TH), eventName, isDynamic: true);
		}

		public void AddSubscription<T, TH>() where T : class, IEvent
			where TH : IEventHandler<T>
		{
			var eventName = GetEventKey<T>();

			AddSubscription(typeof(TH), eventName, isDynamic: false);

			if (!_eventTypes.Contains(typeof(T)))
			{
				_eventTypes.Add(typeof(T));
			}
		}

		public void RemoveSubscription<T, TH>() where T : class, IEvent
			where TH : IEventHandler<T>
		{
			var handlerToRemove = FindSubscription<T, TH>();
			var eventName = GetEventKey<T>();
			RemoveHandler(eventName, handlerToRemove);
		}

		public void RemoveSubscription<TH>(string eventName) where TH : IDynamicEventHandler
		{
			var handlerToRemove = FindSubscription<TH>(eventName);
			RemoveHandler(eventName, handlerToRemove);
		}

		public bool ContainSubscription<T>() where T : class, IEvent
		{
			var key = GetEventKey<T>();
			return ContainSubscription(key);
		}

		public bool ContainSubscription(string eventName) => _handlers.ContainsKey(eventName);

		public Type GetEventType(string eventName) => _eventTypes.SingleOrDefault(t => t.Name == eventName);

		public IEnumerable<SubscriptionInfo> GetHandlers<T>() where T : class, IEvent
		{
			var key = GetEventKey<T>();
			return GetHandlers(key);
		}

		public IEnumerable<SubscriptionInfo> GetHandlers(string eventName) => _handlers[eventName];

		public string GetEventKey<T>() where T : class, IEvent
		{
			return GetEventKey(typeof(T));
		}

		public string GetEventKey(Type eventType)
		{
			return eventType.Name;
		}

		private void AddSubscription(Type handlerType, string eventName, bool isDynamic)
		{
			if (!ContainSubscription(eventName))
			{
				_handlers.Add(eventName, new List<SubscriptionInfo>());
			}

			if (_handlers[eventName].Any(s => s.HandlerType == handlerType))
			{
				throw new ArgumentException(
					$"Handler Type {handlerType.Name} already registered for '{eventName}'", nameof(handlerType));
			}

			_handlers[eventName].Add(isDynamic
				? SubscriptionInfo.CreateDynamic(handlerType)
				: SubscriptionInfo.CreateTyped(handlerType));
		}

		private SubscriptionInfo FindSubscription<TH>(string eventName)
			where TH : IDynamicEventHandler
		{
			return FindSubscription(eventName, typeof(TH));
		}

		private SubscriptionInfo FindSubscription<T, TH>()
			where T : class, IEvent
			where TH : IEventHandler<T>
		{
			var eventName = GetEventKey<T>();
			return FindSubscription(eventName, typeof(TH));
		}

		private SubscriptionInfo FindSubscription(string eventName, Type handlerType)
		{
			if (!ContainSubscription(eventName))
			{
				return null;
			}

			return _handlers[eventName].SingleOrDefault(s => s.HandlerType == handlerType);
		}

		private void RemoveHandler(string eventName, SubscriptionInfo subscription)
		{
			if (subscription != null)
			{
				_handlers[eventName].Remove(subscription);
				if (!_handlers[eventName].Any())
				{
					_handlers.Remove(eventName);
					var eventType = _eventTypes.SingleOrDefault(e => e.Name == eventName);
					if (eventType != null)
					{
						_eventTypes.Remove(eventType);
					}

					OnEventRemoved?.Invoke(this, eventName);
				}
			}
		}
	}
}