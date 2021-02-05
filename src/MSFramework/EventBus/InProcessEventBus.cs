using System;
using System.Threading.Tasks;
using MicroserviceFramework.Shared;

namespace MicroserviceFramework.EventBus
{
	public class InProcessEventBus : IEventBus
	{
		private readonly IEventHandlerFactory _handlerFactory;

		public InProcessEventBus(
			IEventHandlerFactory handlerFactory)
		{
			_handlerFactory = handlerFactory;
		}

		public virtual async Task PublishAsync(dynamic @event)
		{
			Check.NotNull(@event, nameof(@event));

			var type = (Type) @event.GetType();
			if (!type.IsEvent())
			{
				throw new MicroserviceFrameworkException($"类型 {type} 不是事件");
			}

			var eventKey = GetEventKey(type);
			await PublishAsync(eventKey, @event);
		}

		public async Task PublishAsync<TEvent>(TEvent @event) where TEvent : EventBase
		{
			Check.NotNull(@event, nameof(@event));
			var eventKey = GetEventKey(typeof(TEvent));
			await PublishAsync(eventKey, @event);
		}

		public async Task<bool> PublishIfEventAsync(dynamic @event)
		{
			if (@event == null)
			{
				return false;
			}

			var type = @event.GetType();
			if (!type.IsEvent())
			{
				return false;
			}

			var eventKey = GetEventKey(type);
			await PublishAsync(eventKey, @event);
			return true;
		}

		public void Dispose()
		{
		}

		protected virtual string GetEventKey(Type type)
		{
			return type.Name;
		}

		private async Task PublishAsync(string eventKey, object @event)
		{
			var handlerInfos = EventHandlerTypeCache.GetOrDefault(eventKey);
			foreach (var handlerInfo in handlerInfos)
			{
				var handlers = _handlerFactory.Create(handlerInfo.Key);

				foreach (var handler in handlers)
				{
					await Task.Yield();
					var task = (Task) handlerInfo.Value.MethodInfo.Invoke(handler,
						new[] {DeepCopy.DeepCopier.Copy(@event)});
					await task.ConfigureAwait(false);
				}
			}
		}
	}
}