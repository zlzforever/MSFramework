using MicroserviceFramework.Domain;

namespace Ordering.Domain.AggregateRoots.Events;

public class OrderShippedDomainEvent : DomainEvent
{
    public Order Order { get; }

    public OrderShippedDomainEvent(Order order)
    {
        Order = order;
    }
}
