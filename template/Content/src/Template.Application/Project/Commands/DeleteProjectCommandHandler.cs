using System.Threading;
using System.Threading.Tasks;
using MicroserviceFramework;
using MicroserviceFramework.Application.CQRS;
using MicroserviceFramework.EventBus;
using Microsoft.Extensions.Logging;
using Template.Application.Project.Events;
using Template.Domain.Repositories;

namespace Template.Application.Project.Commands
{
	public class DeleteProjectCommandHandler : ICommandHandler<DeleteProjectCommand>
	{
		private readonly IProductRepository _productRepository;
		private readonly ILogger _logger;
		private readonly IEventBus _eventBus;

		public DeleteProjectCommandHandler(
			IProductRepository productRepository,
			ILogger<DeleteProjectCommandHandler> logger, IEventBus eventBus)
		{
			_productRepository = productRepository;
			_logger = logger;
			_eventBus = eventBus;
			_eventBus = eventBus;
		}

		public async Task HandleAsync(DeleteProjectCommand command,
			CancellationToken cancellationToken = new CancellationToken())
		{
			var product = await _productRepository.GetAsync(command.ProjectId);
			if (product != null)
			{
				await _productRepository.DeleteAsync(product);
				await _eventBus.PublishAsync(new ProductDeletedEvent(command.ProjectId));
				_logger.LogInformation($"Delete product {command.ProjectId}");
			}
			else
			{
				throw new MicroserviceFrameworkException(110, "Product is not exists");
			}
		}
	}
}