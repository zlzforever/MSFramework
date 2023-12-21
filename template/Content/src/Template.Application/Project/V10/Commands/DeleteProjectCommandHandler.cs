using System.Threading;
using System.Threading.Tasks;
using MicroserviceFramework;
using MicroserviceFramework.Mediator;
using Microsoft.Extensions.Logging;
using MongoDB.Bson;
using Template.Domain.Repositories;

namespace Template.Application.Project.V10.Commands;

public class DeleteProjectCommandHandler(
    IProductRepository productRepository,
    ILogger<DeleteProjectCommandHandler> logger)
    : IRequestHandler<DeleteProjectCommand, ObjectId>
{
    private readonly ILogger _logger = logger;

    public async Task<ObjectId> HandleAsync(DeleteProjectCommand command,
        CancellationToken cancellationToken = new())
    {
        var product = await productRepository.FindAsync(command.ProjectId);
        if (product != null)
        {
            product.Delete();
            await productRepository.DeleteAsync(product);

            _logger.LogInformation($"Delete product {command.ProjectId}");
        }
        else
        {
            throw new MicroserviceFrameworkException(110, "Product is not exists");
        }

        return product.Id;
    }
}
