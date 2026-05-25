using System.Text.Json;
using DotNetCore.CAP;
using MicroserviceFramework.Domain;
using Microsoft.AspNetCore.Mvc;
using MongoDB.Bson;
using Ordering.Application.Events;
using Ordering.Domain.Repositories;

namespace Ordering.DistributedTransaction.API.Controllers;

[ApiController]
[Route("[controller]")]
public class SubscribeController(
    ILogger<SubscribeController> logger,
    IProductRepository productRepository,
    IUnitOfWork unitOfWork)
    : ControllerBase
{
    [CapSubscribe(Names.ProjectCreatedEvent)]
    [NonAction]
    public async Task CreatedAsync(ProjectCreatedIntegrationEvent @event)
    {
        var product = await productRepository.FindAsync(@event.Id);
        if (product != null)
        {
            product.SetName(Guid.NewGuid().ToString());
        }

        await unitOfWork.SaveChangesAsync();
        logger.LogInformation($"Created: {JsonSerializer.Serialize(@event)}");
    }

    [CapSubscribe(Names.ProjectCreateFailedEvent)]
    [NonAction]
    public Task CreateFailedAsync(ProjectCreatedIntegrationEvent @event)
    {
        throw new ApplicationException("Transaction failed");
        // _logger.LogInformation("Received project created failed event: " + JsonSerializer.Serialize(@event));
        // return Task.CompletedTask;
    }

    public class ProjectCreatedIntegrationEvent
    {
        public ObjectId Id { get; set; }
        public string Name { get; set; }
        public DateTimeOffset CreationTime { get; set; }
    }
}
