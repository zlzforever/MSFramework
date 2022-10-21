using System;
using System.Threading;
using System.Threading.Tasks;
using Dapr.Client;
using MicroserviceFramework.Domain;
using MicroserviceFramework.EventBus;
using MongoDB.Bson;
using Ordering.Domain.AggregateRoots;

namespace Ordering.Application.EventHandlers;

public class ProjectCreatedIntegrationEvent
{
    public ObjectId Id { get; set; }
}

public class ProjectCreateEventHandler : IDomainEventHandler<ProjectCreateEvent>
{
    private readonly DaprClient _daprClient;

    public ProjectCreateEventHandler(DaprClient daprClient)
    {
        _daprClient = daprClient;
    }

    public async Task HandleAsync(ProjectCreateEvent @event, CancellationToken cancellationToken = default)
    {
        Console.WriteLine("Execute ProjectCreateEvent");
        var integrationEvent = new ProjectCreatedIntegrationEvent { Id = @event.Id };

        await _daprClient.PublishEventAsync("pubsub",
            "Ordering.Application.EventHandlers.ProjectCreatedIntegrationEvent", integrationEvent, cancellationToken);
    }

    public void Dispose()
    {
    }
}
