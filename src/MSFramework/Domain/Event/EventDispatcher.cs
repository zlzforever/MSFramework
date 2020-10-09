using System;
using System.Threading.Tasks;
using MicroserviceFramework.Shared;

namespace MicroserviceFramework.Domain.Event
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
			where TEvent : DomainEvent
			where TEventHandler : IEventHandler<TEvent>
		{
			var handlerType = typeof(TEventHandler);
			return Register<TEvent>(handlerType);
		}

		public virtual bool Register<TEvent>(Type handlerType) where TEvent : DomainEvent
		{
			return Register(typeof(TEvent), handlerType);
		}

		public virtual bool Register(Type eventType, Type handlerType)
		{
			Check.NotNull(eventType, nameof(eventType));
			Check.NotNull(handlerType, nameof(handlerType));

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

		public virtual async Task DispatchAsync(DomainEvent @event)
		{
			if (@event == null)
			{
				throw new ArgumentNullException(nameof(@event));
			}

			var eventType = @event.GetType();
			var handlerInfos = _eventHandlerTypeStore.GetHandlers(eventType);
			foreach (var handlerInfo in handlerInfos)
			{
				var handler = _handlerFactory.Create(handlerInfo.Type);
				if (handler != null)
				{
					if (handlerInfo.Method.Invoke(handler, new object[] {@event}) is Task task)
					{
						await task;
					}
				}
				else
				{
					throw new ApplicationException($"Create handler {handlerInfo.Type} object failed");
				}
			}
		}

		public virtual void Dispose()
		{
		}
	}
}