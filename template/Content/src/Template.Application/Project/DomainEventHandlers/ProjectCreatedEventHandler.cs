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
	private readonly IUnitOfWork _unitOfWork;

	public ProjectCreatedEventHandler(DaprClient daprClient, ILogger<ProjectCreatedEventHandler> logger, IUnitOfWork unitOfWork)
	{
		_daprClient = daprClient;
		_logger = logger;
		_unitOfWork = unitOfWork;
	}

	public async Task HandleAsync(ProjectCreatedEvent @event, CancellationToken cancellationToken = default)
	{
		var integrationEvent = new ProjectCreatedIntegrationEvent { Id = @event.Id };
		await _unitOfWork.SaveChangesAsync(cancellationToken);
		await _daprClient.PublishEventAsync("pubsub",
			"ProjectCreatedEvent", integrationEvent,
			cancellationToken: cancellationToken);
		_logger.LogInformation("Execute ProjectCreatedEvent");
	}

	public void Dispose()
	{
	}
}