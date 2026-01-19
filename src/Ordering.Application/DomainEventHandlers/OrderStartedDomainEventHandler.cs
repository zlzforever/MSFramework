using System.Threading;
using System.Threading.Tasks;
using MicroserviceFramework.Domain;
using Microsoft.Extensions.Logging;
using Ordering.Domain.AggregateRoots.Events;

namespace Ordering.Application.DomainEventHandlers;

public class OrderStartedDomainEventHandler(
    ILogger<OrderStartedDomainEventHandler> logger,IUnitOfWork  unitOfWork)
    : IDomainEventHandler<OrderStartedDomainEvent>
{
    public Task HandleAsync(OrderStartedDomainEvent @event, CancellationToken cancellationToken = default)
    {
        unitOfWork.SavedChanges +=
            async () =>
            {
                // await daprClient.PublishEventAsync("rabbitmq-pubsub", Names.OrderCreatedEvent,
                //     new { @event.Id, @event.Name, @event.CreationTime }, cancellationToken: cancellationToken);
                logger.LogInformation("Publish OrderCreatedEvent");
            };
        logger.LogInformation("Publish OrderCreatedEvent");
        return Task.CompletedTask;
    }
}
