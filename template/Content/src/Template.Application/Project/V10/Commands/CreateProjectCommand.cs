using MicroserviceFramework.Mediator;
using Template.Domain.Aggregates.Project;

namespace Template.Application.Project.V10.Commands;

public record CreateProjectCommand : Request<Dto.V10.CreateProductOut>
{
    public string Name { get; set; }
    public int Price { get; set; }
    public ProductType Type { get; set; }
}
