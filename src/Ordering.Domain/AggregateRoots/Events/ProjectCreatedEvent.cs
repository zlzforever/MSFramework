using System;
using MicroserviceFramework.Domain;
using MongoDB.Bson;

namespace Ordering.Domain.AggregateRoots.Events;

public record ProjectCreatedEvent : DomainEvent
{
    public ObjectId Id { get; set; }
    public string Name { get; set; }
    public DateTimeOffset CreationTime { get; set; }
}
