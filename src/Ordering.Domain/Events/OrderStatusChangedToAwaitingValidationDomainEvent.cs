using System;
using Ordering.Domain.AggregateRoot.Order;

namespace Ordering.Domain.Events
{
    using MediatR;
    using System.Collections.Generic;

    /// <summary>
    /// Event used when the grace period order is confirmed
    /// </summary>
    public class OrderStatusChangedToAwaitingValidationDomainEvent
         : INotification
    {
        public Guid OrderId { get; }
        public IEnumerable<OrderItem> OrderItems { get; }

        public OrderStatusChangedToAwaitingValidationDomainEvent(Guid orderId,
            IEnumerable<OrderItem> orderItems)
        {
            OrderId = orderId;
            OrderItems = orderItems;
        }
    }
}