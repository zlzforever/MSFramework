using MicroserviceFramework.Mediator;
using MongoDB.Bson;

namespace Template.Application.Project.IntegrationEvents;

public record ProjectCreatedIntegrationEvent : Request
{
	public ObjectId Id { get; set; }
}