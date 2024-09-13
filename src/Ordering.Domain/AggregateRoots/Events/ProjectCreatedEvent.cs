using System;
using MicroserviceFramework.Domain;

namespace Ordering.Domain.AggregateRoots.Events;

public record ProjectCreatedEvent : DomainEvent
{
    public string Id { get; set; }
    public string Name { get; set; }
    public DateTimeOffset CreationTime { get; set; }
}
