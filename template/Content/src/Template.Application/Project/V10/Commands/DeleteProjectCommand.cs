using MicroserviceFramework.Mediator;
using MongoDB.Bson;

namespace Template.Application.Project.V10.Commands;

public record DeleteProjectCommand : Request<ObjectId>
{
    public ObjectId ProjectId { get; set; }
}
