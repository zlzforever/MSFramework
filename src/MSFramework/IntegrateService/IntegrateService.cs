using System;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using MSFramework.Common;
using MSFramework.EventBus;
using MSFramework.Serialization;

namespace MSFramework.IntegrateService
{
	public class IntegrateService : IIntegrateService
	{
		private readonly IEventBus _eventBus;
		private readonly ILogger _logger;

		public IntegrateService(IEventBus eventBus, ILogger<IntegrateService> logger)
		{
			_eventBus = eventBus;
			_logger = logger;
		}

		public async Task PublishIntegrateEventAsync(IntegrationEvent @event)
		{
			try
			{
				await _eventBus.PublishAsync(@event);
				_logger.LogInformation(
					$"Publish integrate event {Singleton<IJsonConvert>.Instance.SerializeObject(@event)} success");
			}
			catch (Exception e)
			{
				_logger.LogInformation(
					$"Publish integrate event {Singleton<IJsonConvert>.Instance.SerializeObject(@event)} failed: {e}");
			}
		}
	}
}