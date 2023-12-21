using System.Threading;
using System.Threading.Tasks;
using MicroserviceFramework.Domain;
using MicroserviceFramework.Mediator;
using MicroserviceFramework.Utils;
using Microsoft.Extensions.Logging;
using Template.Domain.Aggregates.Project;
using Template.Domain.Repositories;

namespace Template.Application.Project.V10.Commands;

public class CreateProjectCommandHandler
    : IRequestHandler<CreateProjectCommand, Dto.V10.CreateProductOut>
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

    public async Task<Dto.V10.CreateProductOut> HandleAsync(CreateProjectCommand command,
        CancellationToken cancellationToken = new())
    {
        Check.NotNull(command, nameof(command));
        var product = Product.New(command.Name, command.Price, command.Type);

        await _productRepository.AddAsync(product);
        _logger.LogInformation("Create product {ProductName} success", product.Name);
        return _objectAssembler.To<Dto.V10.CreateProductOut>(product);
    }
}
