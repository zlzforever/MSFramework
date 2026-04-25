using System.Threading;
using System.Threading.Tasks;
using Dapr.Client;
using MicroserviceFramework.Domain;
using Microsoft.Extensions.Logging;
using Ordering.Application.Events;
using Ordering.Domain.AggregateRoots.Events;

namespace Ordering.Application.DomainEventHandlers;

public class ProjectCreatedEventHandler(
    ILogger<ProjectCreatedEventHandler> logger,
    IUnitOfWork unitOfWork,
    DaprClient daprClient)
    : IDomainEventHandler<ProjectCreatedEvent>
{
    public Task HandleAsync(ProjectCreatedEvent @event, CancellationToken cancellationToken = default)
    {
        // 领域事件 <-> 和当前上下文/SCOPE 同时成功、失败，同步
        // 集成事件 <-> 异步不管的
        // 领域事件的处理器，发送集成事件
        unitOfWork.SavedChanges +=
            async () =>
            {
                await daprClient.PublishEventAsync("rabbitmq-pubsub", Names.ProjectCreatedEvent,
                    new { @event.Id, @event.Name, @event.CreationTime }, cancellationToken: cancellationToken);
                logger.LogInformation("Publish ProjectCreatedEvent");
            };
        return Task.CompletedTask;
    }
}
