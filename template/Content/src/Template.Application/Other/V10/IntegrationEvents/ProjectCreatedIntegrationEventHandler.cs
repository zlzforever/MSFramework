using System.Threading;
using System.Threading.Tasks;
using MicroserviceFramework.Mediator;
using MicroserviceFramework.Serialization;
using Microsoft.Extensions.Logging;

namespace Template.Application.Other.V10.IntegrationEvents;

public class ProjectCreatedIntegrationEventHandler(
    ILogger<ProjectCreatedIntegrationEventHandler> logger,
    IJsonSerializer jsonSerializer)
    : IRequestHandler<ProjectCreatedIntegrationEvent>
{
    public Task HandleAsync(ProjectCreatedIntegrationEvent request,
        CancellationToken cancellationToken = new())
    {
        logger.LogInformation("Received integration event: {Event}", jsonSerializer.Serialize(request));
        return Task.CompletedTask;
    }
}
