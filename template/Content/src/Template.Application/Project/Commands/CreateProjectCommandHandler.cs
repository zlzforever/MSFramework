using System.Threading;
using System.Threading.Tasks;
using MicroserviceFramework.Application.CQRS;
using MicroserviceFramework.ObjectMapper;
using MicroserviceFramework.Shared;
using Microsoft.Extensions.Logging;
using Template.Application.Project.DTOs;
using Template.Domain.Aggregates.Project;
using Template.Domain.Repositories;

namespace Template.Application.Project.Commands
{
	public class CreateProjectCommandHandler : ICommandHandler<CreateProjectCommand, CreatProductOut>
	{
		private readonly IProductRepository _productRepository;
		private readonly ILogger _logger;
		private readonly IObjMapper _mapper;

		public CreateProjectCommandHandler(IObjMapper mapper,
			IProductRepository productRepository,
			ILogger<CreateProjectCommandHandler> logger)
		{
			_productRepository = productRepository;
			_logger = logger;
			_mapper = mapper;
		}

		public async Task<CreatProductOut> HandleAsync(CreateProjectCommand command,
			CancellationToken cancellationToken = new CancellationToken())
		{
			Check.NotNull(command, nameof(command));
			var product = _mapper.Map<Product>(command);
			var result = await _productRepository.InsertAsync(product);
			_logger.LogInformation($"Create product {result.Name} success");
			return _mapper.Map<CreatProductOut>(result);
		}
	}
}