using System.Threading.Tasks;
using MicroserviceFramework.Serialization;
using MicroserviceFramework.Utilities;

namespace MicroserviceFramework.EventBus
{
	public class InProcessEventBus : IEventBus
	{
		private readonly IEventExecutor _eventExecutor;

		public InProcessEventBus(IEventExecutor eventExecutor)
		{
			_eventExecutor = eventExecutor;
		}

		public async Task PublishAsync<TEvent>(TEvent @event) where TEvent : EventBase
		{
			Check.NotNull(@event, nameof(@event));
			await _eventExecutor.ExecuteAsync(@event.GetType().GetEventName(), Default.JsonHelper.Serialize(@event));
		}

		public void Dispose()
		{
		}
	}
}