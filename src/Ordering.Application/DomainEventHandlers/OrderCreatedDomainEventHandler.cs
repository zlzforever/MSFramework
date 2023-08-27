using System.Threading;
using System.Threading.Tasks;
using Dapr.Client;
using MicroserviceFramework.Domain;
using Microsoft.Extensions.Logging;
using Ordering.Application.Events;
using Ordering.Domain.AggregateRoots.Events;

namespace Ordering.Application.DomainEventHandlers;

public class OrderCreatedDomainEventHandler : IDomainEventHandler<OrderCreatedDomainEvent>
{
    private readonly ILogger<OrderCreatedDomainEventHandler> _logger;
    private readonly IUnitOfWork _unitOfWork;
    private readonly DaprClient _daprClient;

    public OrderCreatedDomainEventHandler(ILogger<OrderCreatedDomainEventHandler> logger,
        IUnitOfWork unitOfWork, DaprClient daprClient)
    {
        _logger = logger;
        _unitOfWork = unitOfWork;
        _daprClient = daprClient;
    }

    public Task HandleAsync(OrderCreatedDomainEvent @event, CancellationToken cancellationToken = default)
    {
        _unitOfWork.Register(async () =>
        {
            await _daprClient.PublishEventAsync("pubsub", Names.OrderCreatedEvent,
               new { @event.Id, @event.Name, @event.CreationTime }, cancellationToken: cancellationToken);
            _logger.LogInformation("Publish OrderCreatedEvent");
        });
        return Task.CompletedTask;
    }
}
