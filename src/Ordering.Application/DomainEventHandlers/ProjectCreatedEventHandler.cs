using System;
using System.Threading;
using System.Threading.Tasks;
using Dapr.Client;
using MicroserviceFramework.Domain;
using MongoDB.Bson;
using Ordering.Domain.AggregateRoots;

namespace Ordering.Application.DomainEventHandlers;

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
        // var json = Defaults.JsonHelper.SerializeToUtf8Bytes(integrationEvent);
        // var str = Encoding.UTF8.GetString(json);
        // var request = _daprClient.CreateInvokeMethodRequest(HttpMethod.Post, "ordering",
        //     "api/v1.0/product/created");
        // request.Content = new ByteArrayContent(json);
        //  await _daprClient.InvokeMethodWithResponseAsync(request, cancellationToken);

        await _daprClient.PublishEventAsync("pubsub",
            "Ordering.Application.EventHandlers.ProjectCreatedIntegrationEvent", integrationEvent
            , cancellationToken);
        
        Console.WriteLine("Execute ProjectCreatedEvent");
    }

    public void Dispose()
    {
    }
}
