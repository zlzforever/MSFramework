using System.Threading.Tasks;
using MicroserviceFramework.EventBus;
using Microsoft.Extensions.Logging;
using Template.Application.Project.Events;

namespace Template.Application.Project.EventHandlers
{
	/// <summary>
	/// this is another scope which is not same with http request
	/// </summary>
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

		public void Dispose()
		{
		}
	}
}