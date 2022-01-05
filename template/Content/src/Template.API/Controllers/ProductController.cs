using System;
using System.Threading.Tasks;
using MicroserviceFramework;
using MicroserviceFramework.AspNetCore;
using MicroserviceFramework.AspNetCore.Mvc;
using MicroserviceFramework.Mediator;
using MicroserviceFramework.Shared;
using Microsoft.AspNetCore.Mvc;
using Template.Application.Project;
#if !DEBUG
using Microsoft.AspNetCore.Authorization;
#endif

namespace Template.API.Controllers
{
	[Route("api/v1.0/product")]
	[ApiController]
#if !DEBUG
	[Authorize]
#endif
	public class ProductController : ApiControllerBase
	{
		private readonly IMediator _mediator;

		public ProductController(
			IMediator mediator)
		{
			_mediator = mediator;
		}

		[HttpGet]
		public async Task<PagedResult<Dtos.V10.ProductOut>> PagedQuery1Async(
			[FromRoute] Queries.V10.PagedProductQuery query)
		{
			var @out = await _mediator.SendAsync(query);
			return @out;
		}

		[HttpPost]
		public async Task<Dtos.V10.CreatProductOut> CreateAsync([FromBody] Commands.V10.CreateProjectCommand command)
		{
			var @out = await _mediator.SendAsync(command);
			return @out;
		}

		[HttpGet("getByName")]
		public async Task<Dtos.V10.ProductOut> GetAsync([FromRoute] Queries.V10.GetProductByNameQuery query)
		{
			return await _mediator.SendAsync(query);
		}

		[HttpDelete]
		public async Task<ApiResult> DeleteAsync([FromRoute] Commands.V10.DeleteProjectCommand command)
		{
			await _mediator.SendAsync(command);
			return Success();
		}

		[HttpGet("Error")]
		public ApiResult GetErrorAsync()
		{
			return new ApiResult("I am an error response");
		}

		[HttpGet("MSFrameworkException")]
		public void GetFrameworkException()
		{
			throw new MicroserviceFrameworkException(2, "i'm framework exception");
		}

		[HttpGet("Exception")]
		public ApiResult GetExceptionAsync()
		{
			throw new Exception("i'm framework exception");
		}
	}
}