using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using MSFramework.Domain.Event;
using Template.Application.Event;

namespace Template.Application.EventHandler
{
	public class ProductDeletedEventHandler : IEventHandler<ProductDeletedEvent>
	{
		private readonly ILogger _logger;

		public ProductDeletedEventHandler(
			ILogger<ProductDeletedEvent> logger)
		{
			_logger = logger;
		}

		public Task HandleAsync(ProductDeletedEvent @event)
		{
			_logger.LogInformation($"Product {@event.ProductId} is deleted");
			return Task.CompletedTask;
		}
	}
}