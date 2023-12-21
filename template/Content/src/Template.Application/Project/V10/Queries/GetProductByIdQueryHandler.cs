using System.Threading;
using System.Threading.Tasks;
using MicroserviceFramework.Domain;
using MicroserviceFramework.Mediator;
using Microsoft.EntityFrameworkCore;
using Template.Domain.Aggregates.Project;
using Template.Infrastructure;

namespace Template.Application.Project.V10.Queries;

public class GetProductByIdQueryHandler(
    TemplateDbContext dbContext,
    IObjectAssembler objectAssembler)
    : IRequestHandler<GetProductByIdQuery, Dto.V10.ProductOut>
{
    public async Task<Dto.V10.ProductOut> HandleAsync(GetProductByIdQuery query,
        CancellationToken cancellationToken = new())
    {
        var product = await dbContext.Set<Product>()
            .FirstOrDefaultAsync(x => x.Id == query.Id, cancellationToken: cancellationToken);
        return objectAssembler.To<Dto.V10.ProductOut>(product);
    }
}
