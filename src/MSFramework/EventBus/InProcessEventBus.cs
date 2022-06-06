using System;
using System.Linq;
using System.Threading.Tasks;
using MicroserviceFramework.Shared;
using Microsoft.Extensions.DependencyInjection;

namespace MicroserviceFramework.EventBus
{
	public class InProcessEventBus : IEventBus
	{
		private readonly IServiceProvider _serviceProvider;

		public InProcessEventBus(IServiceProvider serviceProvider)
		{
			_serviceProvider = serviceProvider;
		}

		public virtual async Task PublishAsync(object @event)
		{
			Check.NotNull(@event, nameof(@event));

			var type = @event.GetType();
			if (!type.IsEvent())
			{
				throw new MicroserviceFrameworkException($"类型 {type} 不是事件");
			}

			await PublishAsync(type.GetEventName(), @event);
		}

		public async Task PublishAsync<TEvent>(TEvent @event) where TEvent : EventBase
		{
			Check.NotNull(@event, nameof(@event));
			await PublishAsync(@event.GetType().GetEventName(), @event);
		}

		public void Dispose()
		{
		}

		private async Task PublishAsync(string eventName, object @event)
		{
			var handlerInfos = EventHandlerTypeCache.GetOrDefault(eventName);
			foreach (var handlerInfo in handlerInfos)
			{
				// TODO: 每次执行是单独的 scope
				await using var scope = _serviceProvider.CreateAsyncScope();
				var handlers = scope.ServiceProvider.GetServices(handlerInfo.Key).ToList();

				foreach (var handler in handlers)
				{
					await Task.Yield();

					if (handlerInfo.Value.MethodInfo.Invoke(handler,
						    new[] { DeepCopy.DeepCopier.Copy(@event) }) is Task task)
					{
						await task.ConfigureAwait(false);
					}
				}
			}
		}
	}
}