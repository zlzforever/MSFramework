using System.Threading;
using System.Threading.Tasks;
using DotNetCore.CAP;
using MicroserviceFramework.Domain;
using Microsoft.Extensions.Logging;
using Ordering.Application.Events;
using Ordering.Domain.AggregateRoots.Events;

namespace Ordering.Application.DomainEventHandlers;

public class ProjectCreatedEventHandler : IDomainEventHandler<ProjectCreatedEvent>
{
    private readonly ICapPublisher _capPublisher;
    private readonly ILogger<ProjectCreatedEventHandler> _logger;

    public ProjectCreatedEventHandler(ICapPublisher capPublisher, ILogger<ProjectCreatedEventHandler> logger)
    {
        _capPublisher = capPublisher;
        _logger = logger;
    }

    public async Task HandleAsync(ProjectCreatedEvent @event, CancellationToken cancellationToken = default)
    {
        await _capPublisher.PublishAsync(Names.ProjectCreatedEvent,
            new 
            {
                @event.Id, @event.Name, @event.CreationTime
            }, cancellationToken: cancellationToken);

        _logger.LogInformation("Publish ProjectCreatedEvent");
    }
}
