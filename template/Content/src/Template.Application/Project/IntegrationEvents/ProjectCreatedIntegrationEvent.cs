using MicroserviceFramework.Mediator;
using MongoDB.Bson;

namespace Template.Application.Project.IntegrationEvents;

public class ProjectCreatedIntegrationEvent : IRequest
{
	public ObjectId Id { get; set; }
}