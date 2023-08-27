using System;
using MicroserviceFramework.Domain;

namespace Ordering.Domain.AggregateRoots.Events;

public record OrderCreatedDomainEvent : DomainEvent
{
    public string Id { get; set; }
    public string Name { get; set; }
    public DateTimeOffset CreationTime { get; set; }
}
