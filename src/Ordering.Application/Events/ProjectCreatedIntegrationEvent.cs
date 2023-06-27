using System;
using MongoDB.Bson;

namespace Ordering.Application.Events;

public class ProjectCreatedIntegrationEvent
{
    public ObjectId Id { get; set; }
    public string Name { get; set; }
    public DateTimeOffset CreationTime { get; set; }
}
