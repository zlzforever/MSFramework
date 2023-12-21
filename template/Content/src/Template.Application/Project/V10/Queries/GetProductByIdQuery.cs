using MicroserviceFramework.Mediator;
using MongoDB.Bson;

namespace Template.Application.Project.V10.Queries;

public record GetProductByIdQuery : Request<Dto.V10.ProductOut>
{
    public ObjectId Id { get; set; }
}
