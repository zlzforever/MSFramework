using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Dapr.Client;
using MicroserviceFramework.Domain;
using MicroserviceFramework.EventBus;
using MicroserviceFramework.Extensions.DependencyInjection;
using MongoDB.Bson;
using Ordering.Domain.AggregateRoots;
using Ordering.Domain.Repositories;

namespace Ordering.Application.EventHandlers;

public class ProjectCreatedIntegrationEvent : EventBase
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
        await _daprClient.PublishEventAsync("pubsub", integrationEvent.GetEventName(), integrationEvent,
            cancellationToken);
    }

    public void Dispose()
    {
    }
}
