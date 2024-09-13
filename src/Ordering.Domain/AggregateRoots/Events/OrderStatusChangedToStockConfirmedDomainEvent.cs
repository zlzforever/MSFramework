using MicroserviceFramework.Domain;

namespace Ordering.Domain.AggregateRoots.Events;

public record OrderStatusChangedToStockConfirmedDomainEvent(string OrderId) : DomainEvent;
