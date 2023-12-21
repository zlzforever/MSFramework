using System.Threading;
using System.Threading.Tasks;
using Dapr.Client;
using MicroserviceFramework.Domain;
using Microsoft.Extensions.Logging;
using Template.Application.Project.V10.IntegrationEvents;
using Template.Domain.Aggregates.Project.Events;

namespace Template.Application.Project.V10.DomainEventHandlers;

public class ProjectCreatedEventHandler(
    DaprClient daprClient,
    ILogger<ProjectCreatedEventHandler> logger,
    IUnitOfWork unitOfWork)
    : IDomainEventHandler<ProjectCreatedEvent>
{
    public async Task HandleAsync(ProjectCreatedEvent @event, CancellationToken cancellationToken = default)
    {
        var integrationEvent = new ProjectCreatedIntegrationEvent { Id = @event.Id };
        unitOfWork.SavedChanges += () => daprClient.PublishEventAsync("pubsub",
            "ProjectCreatedEvent", integrationEvent,
            cancellationToken: cancellationToken);
        await unitOfWork.SaveChangesAsync(cancellationToken);

        logger.LogInformation("Execute ProjectCreatedEvent");
    }

    public void Dispose()
    {
    }
}
