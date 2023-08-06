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
    private readonly ICapPublisher _publisher;
    private readonly ILogger<ProjectCreatedEventHandler> _logger;
    private readonly IUnitOfWork _unitOfWork;

    public ProjectCreatedEventHandler(ICapPublisher capPublisher, ILogger<ProjectCreatedEventHandler> logger,
        IUnitOfWork unitOfWork)
    {
        _publisher = capPublisher;
        _logger = logger;
        _unitOfWork = unitOfWork;
    }

    public Task HandleAsync(ProjectCreatedEvent @event, CancellationToken cancellationToken = default)
    {
        _unitOfWork.Register(async () =>
        {
            await _publisher.PublishAsync(Names.ProjectCreatedEvent,
                new { @event.Id, @event.Name, @event.CreationTime }, cancellationToken: cancellationToken);

            _logger.LogInformation("Publish ProjectCreatedEvent");
        });
        return Task.CompletedTask;
    }
}
