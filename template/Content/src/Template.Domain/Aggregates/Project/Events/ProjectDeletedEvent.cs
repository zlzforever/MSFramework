using MicroserviceFramework.Domain;
using MongoDB.Bson;

namespace Template.Domain.Aggregates.Project.Events;

public class ProjectDeletedEvent
	: DomainEvent
{
	public ObjectId Id { get; set; }
}