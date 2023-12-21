using System.Threading.Tasks;
using Dapr;
using MicroserviceFramework.AspNetCore;
using MicroserviceFramework.Common;
using MicroserviceFramework.Mediator;
using Microsoft.AspNetCore.Mvc;
using MongoDB.Bson;
using Template.Application.Project.V10;
using Template.Application.Project.V10.Commands;
using Template.Application.Project.V10.IntegrationEvents;
using Template.Application.Project.V10.Queries;
#if !DEBUG
using Microsoft.AspNetCore.Authorization;
#endif

namespace Template.API.Controllers;

[Route("api/v1.0/products")]
[ApiController]
#if !DEBUG
    [Authorize]
#endif
public class ProductController(IMediator mediator) : ApiControllerBase
{
    [HttpGet]
    public async Task<PaginationResult<Dto.V10.ProductOut>> PagedQueryAsync(
        [FromRoute] PagedProductQuery query)
    {
        var @out = await mediator.SendAsync(query);
        return @out;
    }

    [HttpPost]
    public async Task<Dto.V10.CreateProductOut> CreateAsync([FromBody] CreateProjectCommand command)
    {
        var @out = await mediator.SendAsync(command);
        return @out;
    }

    [HttpGet("{id}")]
    public Task<Dto.V10.ProductOut> GetAsync([FromRoute] GetProductByIdQuery query)
    {
        return mediator.SendAsync(query);
    }

    [HttpDelete]
    public Task<ObjectId> DeleteAsync([FromRoute] DeleteProjectCommand command)
    {
        return mediator.SendAsync(command);
    }

    [HttpPatch("{id}")]
    public Task<ObjectId> UpdateAsync()
    {
        return Task.FromResult(ObjectId.Empty);
    }

    [Topic("pubsub", "ProjectCreatedEvent")]
    [NonAction]
    public Task SubscribeProjectCreatedEventAsync([FromBody] ProjectCreatedIntegrationEvent @event)
    {
        return mediator.SendAsync(@event);
    }
}
