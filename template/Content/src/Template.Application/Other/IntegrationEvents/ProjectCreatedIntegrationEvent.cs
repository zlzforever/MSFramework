using MicroserviceFramework.Mediator;
using MongoDB.Bson;

namespace Template.Application.Other.IntegrationEvents;

public record ProjectCreatedIntegrationEvent : Request
{
	public ObjectId Id { get; set; }
}