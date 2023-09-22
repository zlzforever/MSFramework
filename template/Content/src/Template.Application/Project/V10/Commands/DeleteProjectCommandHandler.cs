using System.Threading;
using System.Threading.Tasks;
using MicroserviceFramework;
using MicroserviceFramework.Mediator;
using Microsoft.Extensions.Logging;
using MongoDB.Bson;
using Template.Domain.Repositories;

namespace Template.Application.Project.V10.Commands;

public class DeleteProjectCommandHandler : IRequestHandler<DeleteProjectCommand, ObjectId>
{
    private readonly IProductRepository _productRepository;
    private readonly ILogger _logger;

    public DeleteProjectCommandHandler(
        IProductRepository productRepository,
        ILogger<DeleteProjectCommandHandler> logger)
    {
        _productRepository = productRepository;
        _logger = logger;
    }

    public async Task<ObjectId> HandleAsync(DeleteProjectCommand command,
        CancellationToken cancellationToken = new CancellationToken())
    {
        var product = await _productRepository.FindAsync(command.ProjectId);
        if (product != null)
        {
            product.Delete();
            await _productRepository.DeleteAsync(product);

            _logger.LogInformation($"Delete product {command.ProjectId}");
        }
        else
        {
            throw new MicroserviceFrameworkException(110, "Product is not exists");
        }

        return product.Id;
    }
}
