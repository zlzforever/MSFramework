using System.Collections.Generic;
using MicroserviceFramework.Domain;

namespace Ordering.Domain.AggregateRoots.Events;

public record OrderStatusChangedToAwaitingValidationDomainEvent(
    string OrderId,
    IEnumerable<OrderItem> OrderItems) : DomainEvent;
