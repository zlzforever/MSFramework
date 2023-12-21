using MicroserviceFramework.Common;
using MicroserviceFramework.Mediator;

namespace Template.Application.Project.V10.Queries;

public record PagedProductQuery : Request<PaginationResult<Dto.V10.ProductOut>>
{
    public int Page { get; set; }
    public int Limit { get; set; }
    public string Keyword { get; set; }
}
