using MicroserviceFramework.Domain;

namespace Ordering.Domain.AggregateRoots.Events;

public record OrderCancelledDomainEvent(Order.Order Order) : DomainEvent;
