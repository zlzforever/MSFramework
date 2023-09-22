using MicroserviceFramework.Mediator;
using MongoDB.Bson;

namespace Template.Application.Project.V10.IntegrationEvents;

public record ProjectCreatedIntegrationEvent : Request
{
    public ObjectId Id { get; set; }
}
