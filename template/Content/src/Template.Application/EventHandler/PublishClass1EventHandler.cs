using System.Threading.Tasks;
using EventBus;
using Microsoft.Extensions.Logging;
using Template.Application.Event;

namespace Template.Application.EventHandler
{
	public class PublishClass1EventHandler : IEventHandler<PublishClass1Event>
	{
		private readonly ILogger _logger;

		public PublishClass1EventHandler(
			ILogger<PublishClass1EventHandler> logger)
		{
			_logger = logger;
		}

		public Task HandleAsync(PublishClass1Event @event)
		{
			_logger.LogInformation($"Publish class1 {@event.Class1Id}");
			return Task.CompletedTask;
		}
	}
}