using System;
using System.Collections.Generic;
using System.Linq;

namespace MSFramework.EventBus
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

		public void AddSubscription<TEventHandler>(string eventName) where TEventHandler : IDynamicEventHandler
		{
			AddSubscription(typeof(TEventHandler), eventName, isDynamic: true);
		}

		public void AddSubscription<TEvent, TEventHandler>() where TEvent : class, IEvent
			where TEventHandler : IEventHandler<TEvent>
		{
			var eventName = GetEventKey<TEvent>();

			AddSubscription(typeof(TEventHandler), eventName, isDynamic: false);

			if (!_eventTypes.Contains(typeof(TEvent)))
			{
				_eventTypes.Add(typeof(TEvent));
			}
		}

		public void RemoveSubscription<TEvent, TEventHandler>() where TEvent : class, IEvent
			where TEventHandler : IEventHandler<TEvent>
		{
			var handlerToRemove = FindSubscription<TEvent, TEventHandler>();
			var eventName = GetEventKey<TEvent>();
			RemoveHandler(eventName, handlerToRemove);
		}

		public void RemoveSubscription<TEventHandler>(string eventName) where TEventHandler : IDynamicEventHandler
		{
			var handlerToRemove = FindSubscription<TEventHandler>(eventName);
			RemoveHandler(eventName, handlerToRemove);
		}

		public bool ContainSubscription<TEvent>() where TEvent : class, IEvent
		{
			var key = GetEventKey<TEvent>();
			return ContainSubscription(key);
		}

		public bool ContainSubscription(string eventName) => _handlers.ContainsKey(eventName);

		public Type GetEventType(string eventName) => _eventTypes.SingleOrDefault(t => t.Name == eventName);

		public IEnumerable<SubscriptionInfo> GetHandlers<TEvent>() where TEvent : class, IEvent
		{
			var key = GetEventKey<TEvent>();
			return GetHandlers(key);
		}

		public IEnumerable<SubscriptionInfo> GetHandlers(string eventName) => _handlers[eventName];

		public string GetEventKey<TEvent>() where TEvent : class, IEvent
		{
			return GetEventKey(typeof(TEvent));
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
				return;
			}

			_handlers[eventName].Add(isDynamic
				? SubscriptionInfo.CreateDynamic(handlerType)
				: SubscriptionInfo.CreateTyped(handlerType));
		}

		private SubscriptionInfo FindSubscription<TEventHandler>(string eventName)
			where TEventHandler : IDynamicEventHandler
		{
			return FindSubscription(eventName, typeof(TEventHandler));
		}

		private SubscriptionInfo FindSubscription<TEvent, TEventHandler>()
			where TEvent : class, IEvent
			where TEventHandler : IEventHandler<TEvent>
		{
			var eventName = GetEventKey<TEvent>();
			return FindSubscription(eventName, typeof(TEventHandler));
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