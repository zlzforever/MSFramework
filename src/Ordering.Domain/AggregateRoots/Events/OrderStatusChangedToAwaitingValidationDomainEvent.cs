using System.Collections.Generic;
using MicroserviceFramework.Domain;
using MongoDB.Bson;

namespace Ordering.Domain.AggregateRoots.Events;

public record OrderStatusChangedToAwaitingValidationDomainEvent(
    ObjectId OrderId,
    IEnumerable<OrderItem> OrderItems) : DomainEvent;
