using MediatR;
using System;
using System.Collections.Generic;
using System.Text;
using MSFramework.Domain;
using Ordering.Domain.AggregateRoot.Order;

namespace Ordering.Domain.Events
{
    public class OrderCancelledDomainEvent : DomainEventBase<Guid>
    {
        public Order Order { get; }

        public OrderCancelledDomainEvent(Order order)
        {
            Order = order;
        }
    }
}
