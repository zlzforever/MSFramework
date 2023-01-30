using System.Threading;
using System.Threading.Tasks;
using DotNetCore.CAP;
using MicroserviceFramework.Domain;
using Microsoft.Extensions.Logging;
using Template.Application.Project.IntegrationEvents;
using Template.Domain.Aggregates.Project.Events;

namespace Template.Application.Project.DomainEventHandlers;

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
		var integrationEvent = new ProjectCreatedIntegrationEvent { Id = @event.Id };

		await _capPublisher.PublishAsync(
			"Template.Application.Product.SubscribeService.ProjectCreatedIntegrationEvent", integrationEvent,
			cancellationToken: cancellationToken);
		_logger.LogInformation("Execute ProjectCreatedEvent");
	}

	public void Dispose()
	{
	}
}