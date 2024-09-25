using System.Collections.Generic;
using MicroserviceFramework.Domain;
using Ordering.Domain.AggregateRoots.Order;

namespace Ordering.Domain.AggregateRoots.Events;

public record OrderStatusChangedToAwaitingValidationDomainEvent(
    string OrderId,
    IEnumerable<OrderItem> OrderItems) : DomainEvent;
