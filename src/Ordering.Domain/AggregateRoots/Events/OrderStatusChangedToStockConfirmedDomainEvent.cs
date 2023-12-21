using MicroserviceFramework.Domain;
using MongoDB.Bson;

namespace Ordering.Domain.AggregateRoots.Events;

public record OrderStatusChangedToStockConfirmedDomainEvent(ObjectId OrderId) : DomainEvent;
