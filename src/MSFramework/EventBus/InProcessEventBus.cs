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

		public virtual async Task PublishAsync(EventBase @event)
		{
			Check.NotNull(@event, nameof(@event));

			var eventName = @event.GetType().Name;
			var handlerInfos = EventHandlerTypeCache.GetOrDefault(eventName);
			foreach (var handlerInfo in handlerInfos)
			{
				var handlers = _handlerFactory.Create(handlerInfo.HandlerType);

				foreach (var handler in handlers)
				{
					await Task.Yield();
					var task = (Task) handlerInfo.MethodInfo.Invoke(handler,
						new object[] {DeepCopy.DeepCopier.Copy(@event)});
					await task.ConfigureAwait(false);
				}
			}
		}

		public void Dispose()
		{
		}
	}
}