using MicroserviceFramework.Domain;

namespace Ordering.Domain.AggregateRoots.Events;

public record OrderStartedDomainEvent(Order Order, string UserId) : DomainEvent;
