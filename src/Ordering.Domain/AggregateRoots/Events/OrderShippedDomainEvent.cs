using MicroserviceFramework.Domain;

namespace Ordering.Domain.AggregateRoots.Events;

public record OrderShippedDomainEvent(Order Order) : DomainEvent;
