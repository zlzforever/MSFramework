using System.Threading;
using System.Threading.Tasks;
using MicroserviceFramework.Domain;
using MicroserviceFramework.Mediator;
using MicroserviceFramework.Utilities;
using Microsoft.Extensions.Logging;
using Template.Domain.Aggregates.Project;
using Template.Domain.Repositories;

namespace Template.Application.Project.CommandHandlers
{
	public class
		CreateProjectCommandHandler : IRequestHandler<Commands.V10.CreateProjectCommand, Dtos.V10.CreatProductOut>
	{
		private readonly IProductRepository _productRepository;
		private readonly ILogger _logger;
		private readonly IObjectAssembler _objectAssembler;

		public CreateProjectCommandHandler(IObjectAssembler objectAssembler,
			IProductRepository productRepository,
			ILogger<CreateProjectCommandHandler> logger)
		{
			_productRepository = productRepository;
			_logger = logger;
			_objectAssembler = objectAssembler;
			_objectAssembler = objectAssembler;
		}

		public async Task<Dtos.V10.CreatProductOut> HandleAsync(Commands.V10.CreateProjectCommand command,
			CancellationToken cancellationToken = new CancellationToken())
		{
			Check.NotNull(command, nameof(command));
			var product = Product.New(command.Name, command.Price, command.Type);

			await _productRepository.AddAsync(product);
			_logger.LogInformation($"Create product {product.Name} success");
			return _objectAssembler.To<Dtos.V10.CreatProductOut>(product);
		}
	}
}