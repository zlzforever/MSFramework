using System;
using System.Threading;
using System.Threading.Tasks;
using Dapr.Client;
using MicroserviceFramework.Domain;
using Microsoft.Extensions.Logging;
using Template.Application.Project.IntegrationEvents;
using Template.Domain.Aggregates.Project.Events;

namespace Template.Application.Project.DomainEventHandlers;

public class ProjectCreatedEventHandler : IDomainEventHandler<ProjectCreatedEvent>
{
	private readonly DaprClient _daprClient;
	private readonly ILogger<ProjectCreatedEventHandler> _logger;

	public ProjectCreatedEventHandler(DaprClient daprClient, ILogger<ProjectCreatedEventHandler> logger)
	{
		_daprClient = daprClient;
		_logger = logger;
	}

	public async Task HandleAsync(ProjectCreatedEvent @event, CancellationToken cancellationToken = default)
	{
		var integrationEvent = new ProjectCreatedIntegrationEvent { Id = @event.Id };

		await _daprClient.PublishEventAsync("pubsub",
			"Ordering.Application.EventHandlers.ProjectCreatedIntegrationEvent", integrationEvent, cancellationToken);
		_logger.LogInformation("Execute ProjectCreatedEvent");
	}

	public void Dispose()
	{
	}
}