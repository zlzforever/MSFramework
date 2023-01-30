using System;
using System.Threading.Tasks;
using Dapr;
using DotNetCore.CAP;
using MicroserviceFramework.AspNetCore;
using MicroserviceFramework.Common;
using MicroserviceFramework.Mediator;
using Microsoft.AspNetCore.Mvc;
using MongoDB.Bson;
using Template.API.Filters;
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
		public async Task<PagedResult<Dto.V10.ProductOut>> PagedQueryAsync(
			[FromRoute] Queries.V10.PagedProductQuery query)
		{
			var @out = await _mediator.SendAsync(query);
			return @out;
		}

		[HttpPost]
		[CapTransaction]
		public async Task<Dto.V10.CreateProductOut> CreateAsync([FromBody] Commands.V10.CreateProjectCommand command)
		{
			var @out = await _mediator.SendAsync(command);
			return @out;
		}

		[HttpGet("{id}")]
		public async Task<Dto.V10.ProductOut> GetAsync([FromRoute] Queries.V10.GetProductByIdQuery query)
		{
			return await _mediator.SendAsync(query);
		}

		[HttpDelete]
		public async Task<ObjectId> DeleteAsync([FromRoute] Commands.V10.DeleteProjectCommand command)
		{
			return await _mediator.SendAsync(command);
		}

		[HttpPatch("{id}")]
		public Task<ObjectId> UpdateAsync()
		{
			return Task.FromResult(ObjectId.Empty);
		}

		[HttpPost("CAP")]
		[CapTransaction]
		public IActionResult EntityFrameworkWithTransaction([FromServices] TemplateDbContext dbContext)
		{
			_capBus.Publish("Ordering.Application.EventHandlers.ProjectCreatedIntegrationEvent", DateTime.Now);

			return Ok();
		}
	}
}