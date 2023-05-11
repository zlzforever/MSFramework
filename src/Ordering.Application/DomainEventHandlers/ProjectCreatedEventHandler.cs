using System;
using System.Threading;
using System.Threading.Tasks;
using DotNetCore.CAP;
using MicroserviceFramework.Domain;
using MongoDB.Bson;
using Ordering.Application.Events;
using Ordering.Domain.AggregateRoots;
using Ordering.Domain.AggregateRoots.Events;

namespace Ordering.Application.DomainEventHandlers;



public class ProjectCreatedEventHandler : IDomainEventHandler<ProjectCreatedEvent>
{
    private readonly ICapPublisher _capPublisher;

    public ProjectCreatedEventHandler(ICapPublisher capPublisher)
    {
        _capPublisher = capPublisher;
    }

    public async Task HandleAsync(ProjectCreatedEvent @event, CancellationToken cancellationToken = default)
    {
        var integrationEvent = new ProjectCreatedIntegrationEvent
        {
            Id = @event.Id,
            Name = @event.Name,
            CreationTime = @event.CreationTime
        };

        await _capPublisher.PublishAsync("Ordering.Application.EventHandlers.ProjectCreatedIntegrationEvent",
            integrationEvent, cancellationToken: cancellationToken);

        Console.WriteLine("Execute ProjectCreatedEvent");
    }

    public void Dispose()
    {
    }
}
