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

    private readonly ILogger<ProjectCreatedEventHandler> _logger;
    private readonly IUnitOfWork _unitOfWork;

    public ProjectCreatedEventHandler(  ILogger<ProjectCreatedEventHandler> logger,
        IUnitOfWork unitOfWork)
    {

        _logger = logger;
        _unitOfWork = unitOfWork;
    }

    public Task HandleAsync(ProjectCreatedEvent @event, CancellationToken cancellationToken = default)
    {
        // _unitOfWork.Register(async () =>
        // {
        //     await _publisher.PublishAsync(Names.ProjectCreatedEvent,
        //         new { @event.Id, @event.Name, @event.CreationTime }, cancellationToken: cancellationToken);
        //
        //     _logger.LogInformation("Publish ProjectCreatedEvent");
        // });
        return Task.CompletedTask;
    }
}
