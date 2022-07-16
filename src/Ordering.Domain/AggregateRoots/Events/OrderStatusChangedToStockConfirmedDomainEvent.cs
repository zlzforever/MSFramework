using MicroserviceFramework.Domain;
using MongoDB.Bson;

namespace Ordering.Domain.AggregateRoots.Events
{
    public class OrderStatusChangedToStockConfirmedDomainEvent
        : DomainEvent
    {
        public ObjectId OrderId { get; }

        public OrderStatusChangedToStockConfirmedDomainEvent(ObjectId orderId)
            => OrderId = orderId;
    }
}