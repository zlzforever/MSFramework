using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using MicroserviceFramework.Common;
using MicroserviceFramework.Domain;
using MicroserviceFramework.Linq.Expression;
using MicroserviceFramework.Mediator;
using Template.Domain.Aggregates.Project;
using Template.Infrastructure;

namespace Template.Application.Project.V10.Queries;

public class PagedProductQueryHandler(TemplateDbContext dbContext, IObjectAssembler objectAssembler)
    : IRequestHandler<PagedProductQuery, PaginationResult<Dto.V10.ProductOut>>
{
    public async Task<PaginationResult<Dto.V10.ProductOut>> HandleAsync(PagedProductQuery query,
        CancellationToken cancellationToken = new())
    {
        query.Keyword = query.Keyword?.Trim();

        PaginationResult<Product> result;
        if (string.IsNullOrWhiteSpace(query.Keyword))
        {
            result = await dbContext.Set<Product>()
                .OrderByDescending(x => x.LastModificationTime)
                .PagedQueryAsync(query.Page, query.Limit);
        }
        else
        {
            result = await dbContext.Set<Product>()
                .Where(x => x.Name.Contains(query.Keyword))
                .OrderByDescending(x => x.LastModificationTime)
                .PagedQueryAsync(query.Page, query.Limit);
        }

        return new PaginationResult<Dto.V10.ProductOut>(result.Page, result.Limit, result.Total,
            objectAssembler.To<List<Dto.V10.ProductOut>>(result.Data));
    }
}
