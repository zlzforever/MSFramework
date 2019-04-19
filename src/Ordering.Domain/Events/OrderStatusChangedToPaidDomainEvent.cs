using System;
using Ordering.Domain.AggregateRoot.Order;

namespace Ordering.Domain.Events
{
    using MediatR;
    using System.Collections.Generic;

    /// <summary>
    /// Event used when the order is paid
    /// </summary>
    public class OrderStatusChangedToPaidDomainEvent
        : INotification
    {
        public Guid OrderId { get; }
        public IEnumerable<OrderItem> OrderItems { get; }

        public OrderStatusChangedToPaidDomainEvent(Guid orderId,
            IEnumerable<OrderItem> orderItems)
        {
            OrderId = orderId;
            OrderItems = orderItems;
        }
    }
}