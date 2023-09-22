using MicroserviceFramework.Mediator;
using MongoDB.Bson;

namespace Template.Application.Other.V10.IntegrationEvents;

public record ProjectCreatedIntegrationEvent : Request
{
    public ObjectId Id { get; set; }
}
