using MediatR;
using System;
using System.Collections.Generic;
using System.Text;
using Ordering.Domain.AggregateRoot.Order;

namespace Ordering.Domain.Events
{
    public class OrderCancelledDomainEvent : INotification
    {
        public Order Order { get; }

        public OrderCancelledDomainEvent(Order order)
        {
            Order = order;
        }
    }
}
