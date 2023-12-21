using System.Threading;
using System.Threading.Tasks;
using Dapr.Client;
using MicroserviceFramework.Domain;
using Microsoft.Extensions.Logging;
using Ordering.Application.Events;
using Ordering.Domain.AggregateRoots.Events;

namespace Ordering.Application.DomainEventHandlers;

public class ProjectCreatedEventHandler(
    ILogger<ProjectCreatedEventHandler> logger,
    IUnitOfWork unitOfWork,
    DaprClient daprClient)
    : IDomainEventHandler<ProjectCreatedEvent>
{
    public Task HandleAsync(ProjectCreatedEvent @event, CancellationToken cancellationToken = default)
    {
        unitOfWork.SavedChanges +=
            async () =>
            {
                await daprClient.PublishEventAsync("rabbitmq-pubsub", Names.ProjectCreatedEvent,
                    new { @event.Id, @event.Name, @event.CreationTime }, cancellationToken: cancellationToken);
                logger.LogInformation("Publish ProjectCreatedEvent");
            };
        return Task.CompletedTask;
    }
}
