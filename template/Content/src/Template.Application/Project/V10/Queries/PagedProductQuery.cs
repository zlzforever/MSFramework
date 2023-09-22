using MicroserviceFramework.Common;
using MicroserviceFramework.Mediator;

namespace Template.Application.Project.V10.QueryHandlers;

public record PagedProductQuery : Request<PagedResult<Dto.V10.ProductOut>>
{
    public int Page { get; set; }
    public int Limit { get; set; }
    public string Keyword { get; set; }
}
