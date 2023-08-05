using MicroserviceFramework.Domain;

namespace Ordering.Domain.AggregateRoots.Events;

public record OrderShippedDomainEvent : DomainEvent
{
    public Order Order { get; }

    public OrderShippedDomainEvent(Order order)
    {
        Order = order;
    }
}
