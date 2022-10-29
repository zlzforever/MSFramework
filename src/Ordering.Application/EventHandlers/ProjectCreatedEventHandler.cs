using System;
using System.Threading;
using System.Threading.Tasks;
using Dapr.Client;
using MicroserviceFramework.Domain;
using MongoDB.Bson;
using Ordering.Domain.AggregateRoots;

namespace Ordering.Application.EventHandlers;

public class ProjectCreatedIntegrationEvent
{
    public ObjectId Id { get; set; }
}

public class ProjectCreatedEventHandler : IDomainEventHandler<ProjectCreatedEvent>
{
    private readonly DaprClient _daprClient;

    public ProjectCreatedEventHandler(DaprClient daprClient)
    {
        _daprClient = daprClient;
    }

    public async Task HandleAsync(ProjectCreatedEvent @event, CancellationToken cancellationToken = default)
    {
        var integrationEvent = new ProjectCreatedIntegrationEvent { Id = @event.Id };

        // await _daprClient.InvokeMethodAsync("sales-collection","api/v1.0/sales-collections")
        await _daprClient.PublishEventAsync("pubsub",
            "Ordering.Application.EventHandlers.ProjectCreatedIntegrationEvent", integrationEvent, cancellationToken);
        Console.WriteLine("Execute ProjectCreatedEvent");
    }

    public void Dispose()
    {
    }
}
