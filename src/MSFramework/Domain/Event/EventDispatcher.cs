using System;
using System.Threading.Tasks;

namespace MSFramework.Domain.Event
{
	public class EventDispatcher : IEventDispatcher
	{
		private readonly IEventHandlerTypeStore _eventHandlerTypeStore;
		private readonly IHandlerFactory _handlerFactory;

		public EventDispatcher(IEventHandlerTypeStore eventHandlerTypeStore,
			IHandlerFactory handlerFactory)
		{
			_handlerFactory = handlerFactory;
			_eventHandlerTypeStore = eventHandlerTypeStore ?? new EventHandlerTypeStore();
		}

		public virtual bool Register<TEvent, TEventHandler>()
			where TEvent : EventBase
			where TEventHandler : IEventHandler<TEvent>
		{
			var handlerType = typeof(TEventHandler);
			return Register<TEvent>(handlerType);
		}

		public virtual bool Register<TEvent>(Type handlerType) where TEvent : EventBase
		{
			return Register(typeof(TEvent), handlerType);
		}

		public virtual bool Register(Type eventType, Type handlerType)
		{
			if (eventType == null)
			{
				throw new ArgumentNullException(nameof(eventType));
			}

			if (handlerType == null)
			{
				throw new ArgumentNullException(nameof(handlerType));
			}

			if (!eventType.IsEvent())
			{
				throw new ArgumentException("Event should inherit from Event and be a class ");
			}

			if (handlerType.CanHandle(eventType))
			{
				_eventHandlerTypeStore.Add(eventType, handlerType);
				return true;
			}
			else
			{
				throw new ArgumentException($"Type {handlerType} is not a valid handler");
			}
		}

		public virtual void Dispatch(IEvent @event)
		{
			if (@event == null)
			{
				throw new ArgumentNullException(nameof(@event));
			}

			var eventType = @event.GetType();
			var handlerTypes = _eventHandlerTypeStore.GetHandlerTypes(eventType);
			foreach (var handlerType in handlerTypes)
			{
				var methodInfo = handlerType.GetMethod("HandleAsync");
				if (methodInfo != null)
				{
					var handler = _handlerFactory.Create(handlerType);
					if (handler != null)
					{
						if (methodInfo.Invoke(handler, new object[] {@event}) is Task task)
						{
							task.ConfigureAwait(false);
						}
						else
						{
							throw new ApplicationException($"Handle method is not async method");
						}
					}
					else
					{
						throw new ApplicationException($"Create handler {handlerType} object failed");
					}
				}
			}
		}
	}
}