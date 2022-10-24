using System;
using System.Threading.Tasks;
using Dapr;
using DotNetCore.CAP;
using MicroserviceFramework.AspNetCore;
using MicroserviceFramework.Common;
using MicroserviceFramework.Mediator;
using Microsoft.AspNetCore.Mvc;
using MongoDB.Bson;
using Template.Application.Project;
using Template.Application.Project.IntegrationEvents;
using Template.Infrastructure;
#if !DEBUG
using Microsoft.AspNetCore.Authorization;
#endif

namespace Template.API.Controllers
{
	[Route("api/v1.0/products")]
	[ApiController]
#if !DEBUG
	[Authorize]
#endif
	public class ProductController : ApiControllerBase
	{
		private readonly IMediator _mediator;
		private readonly ICapPublisher _capBus;

		public ProductController(
			IMediator mediator, ICapPublisher capBus)
		{
			_mediator = mediator;
			_capBus = capBus;
		}

		[HttpGet]
		public async Task<PagedResult<Dtos.V10.ProductOut>> PagedQueryAsync(
			[FromRoute] Queries.V10.PagedProductQuery query)
		{
			var @out = await _mediator.SendAsync(query);
			return @out;
		}

		[HttpPost]
		public async Task<Dtos.V10.CreateProductOut> CreateAsync([FromBody] Commands.V10.CreateProjectCommand command)
		{
			var @out = await _mediator.SendAsync(command);
			return @out;
		}

		[HttpGet("{id}")]
		public async Task<Dtos.V10.ProductOut> GetAsync([FromRoute] Queries.V10.GetProductByIdQuery query)
		{
			return await _mediator.SendAsync(query);
		}

		[HttpDelete]
		public async Task<ObjectId> DeleteAsync([FromRoute] Commands.V10.DeleteProjectCommand command)
		{
			return await _mediator.SendAsync(command);
		}

		[Topic("pubsub", "Template.Application.Project.IntegrationEvents.ProjectCreatedIntegrationEvent")]
		[HttpPost("created")]
		public async Task<ObjectId> CreatedAsync([FromBody] ProjectCreatedIntegrationEvent @event)
		{
			await _mediator.SendAsync(@event);
			return @event.Id;
		}

		[HttpPost("CAP")]
		public IActionResult EntityFrameworkWithTransaction([FromServices] TemplateDbContext dbContext)
		{
			dbContext.Database.BeginTransaction(_capBus, autoCommit: true);
			_capBus.Publish("Ordering.Application.EventHandlers.ProjectCreatedIntegrationEvent", DateTime.Now);

			return Ok();
		}

		[CapSubscribe("Ordering.Application.EventHandlers.ProjectCreatedIntegrationEvent")]
		[NonAction]
		public void CheckReceivedMessage(DateTime datetime)
		{
			Console.WriteLine(datetime);
		}
	}
}