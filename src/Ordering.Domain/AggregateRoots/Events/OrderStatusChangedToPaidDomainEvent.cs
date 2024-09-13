using System.Collections.Generic;
using MicroserviceFramework.Domain;

namespace Ordering.Domain.AggregateRoots.Events;

public record OrderStatusChangedToPaidDomainEvent(
    string OrderId,
    IEnumerable<OrderItem> OrderItems) : DomainEvent;
