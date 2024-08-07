using System.Threading;
using System.Threading.Tasks;
using Dapr.Client;
using MicroserviceFramework.Domain;
using Microsoft.Extensions.Logging;
using Ordering.Domain.AggregateRoots.Events;

namespace Ordering.Application.DomainEventHandlers;

public class OrderCreatedDomainEventHandler(
    ILogger<OrderCreatedDomainEventHandler> logger,
    IUnitOfWork unitOfWork,
    DaprClient daprClient)
    : IDomainEventHandler<OrderCreatedDomainEvent>
{
    public Task HandleAsync(OrderCreatedDomainEvent @event, CancellationToken cancellationToken = default)
    {
        unitOfWork.SavedChanges +=
            async () =>
            {
                // await daprClient.PublishEventAsync("rabbitmq-pubsub", Names.OrderCreatedEvent,
                //     new { @event.Id, @event.Name, @event.CreationTime }, cancellationToken: cancellationToken);
                logger.LogInformation("Publish OrderCreatedEvent");
            };
        return Task.CompletedTask;
    }
}
