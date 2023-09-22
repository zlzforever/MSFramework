using System.Threading;
using System.Threading.Tasks;
using MicroserviceFramework;
using MicroserviceFramework.Mediator;
using Microsoft.Extensions.Logging;

namespace Template.Application.Other.V10.IntegrationEvents
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
            _logger.LogInformation("Received integration event: {Event}", Defaults.JsonSerializer.Serialize(request));
            return Task.CompletedTask;
        }
    }
}
