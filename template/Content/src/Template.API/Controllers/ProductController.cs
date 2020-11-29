using System;
using System.Threading.Tasks;
using MicroserviceFramework;
using MicroserviceFramework.Application.CQRS;
using MicroserviceFramework.AspNetCore;
using MicroserviceFramework.AspNetCore.Mvc;
using MicroserviceFramework.Shared;
using Microsoft.AspNetCore.Mvc;
#if !DEBUG
using Microsoft.AspNetCore.Authorization;
#endif
using Template.Application.Project.Commands;
using Template.Application.Project.DTOs;
using Template.Application.Project.Queries;

namespace Template.API.Controllers
{
	[Route("api/v1.0/product")]
	[ApiController]
#if !DEBUG
	[Authorize]
#endif
	public class ProductController : ApiControllerBase
	{
		private readonly ICqrsProcessor _cqrsProcessor;

		public ProductController(
			ICqrsProcessor cqrsProcessor)
		{
			_cqrsProcessor = cqrsProcessor;
		}

		[HttpGet]
		public async Task<PagedResult<ProductOut>> PagedQuery1Async([FromRoute] PagedProductQuery query)
		{
			var @out = await _cqrsProcessor.QueryAsync(query);
			return @out;
		}

		[HttpPost]
		public async Task<CreatProductOut> CreateAsync([FromBody] CreateProjectCommand command)
		{
			var result = await _cqrsProcessor.ExecuteAsync(command);
			return result;
		}

		[HttpGet("getByName")]
		public async Task<ProductOut> GetAsync([FromRoute] GetProductByNameQuery query)
		{
			return await _cqrsProcessor.QueryAsync(query);
		}

		[HttpDelete]
		public async Task<Response> DeleteAsync([FromRoute] DeleteProjectCommand command)
		{
			await _cqrsProcessor.ExecuteAsync(command);
			return Success();
		}

		[HttpGet("Error")]
		public Response GetErrorAsync()
		{
			return new ErrorResponse("I am an error response");
		}

		[HttpGet("MSFrameworkException")]
		public void GetFrameworkException()
		{
			throw new MicroserviceFrameworkException(2, "i'm framework exception");
		}

		[HttpGet("Exception")]
		public Response GetExceptionAsync()
		{
			throw new Exception("i'm framework exception");
		}
	}
}