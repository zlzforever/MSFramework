using System.Threading;
using System.Threading.Tasks;
using MicroserviceFramework.Domain;
using MicroserviceFramework.Ef;
using Ordering.Domain.AggregateRoots.Events;

namespace Ordering.Application.DomainEventHandlers;

public class OrderShippedDomainEventHandler(DbContextFactory dbContextFactory)
    : IDomainEventHandler<OrderShippedDomainEvent>
{
    private readonly DbContextFactory _dbContextFactory = dbContextFactory;

    public void Dispose()
    {
    }

    public Task HandleAsync(OrderShippedDomainEvent query, CancellationToken cancellationToken = default)
    {
        // todo
        return Task.CompletedTask;
    }
}
