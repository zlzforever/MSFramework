using System.Threading;
using System.Threading.Tasks;
using MicroserviceFramework.Domain;
using MicroserviceFramework.Mediator;
using MicroserviceFramework.Utils;
using Microsoft.Extensions.Logging;
using Template.Domain.Aggregates.Project;
using Template.Domain.Repositories;

namespace Template.Application.Project.CommandHandlers
{
	public class CreateProjectCommandHandler
		: IRequestHandler<Commands.V10.CreateProjectCommand, Dto.V10.CreateProductOut>
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

		public async Task<Dto.V10.CreateProductOut> HandleAsync(Commands.V10.CreateProjectCommand command,
			CancellationToken cancellationToken = new CancellationToken())
		{
			Check.NotNull(command, nameof(command));
			var product = Product.New(command.Name, command.Price, command.Type);

			await _productRepository.AddAsync(product);
			_logger.LogInformation("Create product {ProductName} success", product.Name);
			return _objectAssembler.To<Dto.V10.CreateProductOut>(product);
		}
	}
}