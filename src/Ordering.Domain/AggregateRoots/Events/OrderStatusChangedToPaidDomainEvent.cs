using MicroserviceFramework.Domain;
using Ordering.Domain.AggregateRoots.Order;

namespace Ordering.Domain.AggregateRoots.Events;

public record OrderStatusChangedToPaidDomainEvent(
    string OrderId,
    IEnumerable<OrderItem> OrderItems) : DomainEvent;
