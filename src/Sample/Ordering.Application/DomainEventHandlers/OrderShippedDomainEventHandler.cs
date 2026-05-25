using System.Threading;
using System.Threading.Tasks;
using MicroserviceFramework.Domain;
using Ordering.Domain.AggregateRoots.Events;

namespace Ordering.Application.DomainEventHandlers;

public class OrderShippedDomainEventHandler()
    : IDomainEventHandler<OrderShippedDomainEvent>
{

    public void Dispose()
    {
    }

    public Task HandleAsync(OrderShippedDomainEvent query, CancellationToken cancellationToken = default)
    {
        // todo
        return Task.CompletedTask;
    }
}
