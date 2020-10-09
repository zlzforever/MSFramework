using System;
using System.Threading.Tasks;
using MicroserviceFramework.Shared;

namespace MicroserviceFramework.Domain.Events
{
	public class EventDispatcher : IEventDispatcher
	{
		private readonly IEventHandlerTypeStore _eventHandlerTypeStore;
		private readonly IHandlerFactory _handlerFactory;
		public EventDispatcher(IEventHandlerTypeStore eventHandlerTypeStore, IHandlerFactory handlerFactory)
		{
			_handlerFactory = handlerFactory;
			_eventHandlerTypeStore = eventHandlerTypeStore ?? new EventHandlerTypeStore();
		}

		public virtual bool Register<TEvent, TEventHandler>()
			where TEvent : Event
			where TEventHandler : IEventHandler<TEvent>
		{
			var handlerType = typeof(TEventHandler);
			return Register<TEvent>(handlerType);
		}

		public virtual bool Register<TEvent>(Type handlerType) where TEvent : Event
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
				_eventHandlerTypeStore.Add(eventType.Name, handlerType);
				return true;
			}
			else
			{
				throw new ArgumentException($"Type {handlerType} is not a valid handler");
			}
		}

		public virtual async Task DispatchAsync(IEvent @event)
		{
			if (@event == null)
			{
				throw new ArgumentNullException(nameof(@event));
			}

			var eventType = @event.GetType();
			var tuples = _eventHandlerTypeStore.GetHandlerTypes(eventType.Name);
			foreach (var tuple in tuples)
			{
				var handler = _handlerFactory.Create(tuple.Key);
				if (handler != null)
				{
					if (tuple.Value.Invoke(handler, new object[] {@event}) is Task task)
					{
						await task;
					}
				}
				else
				{
					throw new ApplicationException($"Create handler {tuple.Key} object failed");
				}
			}
		}

		public virtual void Dispose()
		{
		}
	}
}