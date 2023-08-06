using System.Threading;
using System.Threading.Tasks;
using MicroserviceFramework;
using MicroserviceFramework.Mediator;
using Microsoft.Extensions.Logging;
using Template.Application.Other.IntegrationEvents;

namespace Template.Application.Other.IntegrationEventHandlers
{
	public class ProjectCreatedIntegrationEventHandler : IRequestHandler<ProjectCreatedIntegrationEvent>
	{
		private readonly ILogger<ProjectCreatedIntegrationEventHandler> _logger;

		public ProjectCreatedIntegrationEventHandler(ILogger<ProjectCreatedIntegrationEventHandler> logger)
		{
			_logger = logger;
		}

		public Task HandleAsync(ProjectCreatedIntegrationEvent request,
			CancellationToken cancellationToken = new CancellationToken())
		{
			_logger.LogInformation("Received integration event: {Event}", Defaults.JsonHelper.Serialize(request));
			return Task.CompletedTask;
		}
	}
}