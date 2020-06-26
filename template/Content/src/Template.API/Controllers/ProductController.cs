using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using MSFramework;
using MSFramework.AspNetCore;
using MSFramework.AspNetCore.Api;
using MSFramework.Data;
using MSFramework.Extensions;
using MSFramework.Mapper;
#if !DEBUG
using Microsoft.AspNetCore.Authorization;
#endif
using Template.API.ViewObject;
using Template.Application.DTO;
using Template.Application.Query;
using Template.Application.Service;

namespace Template.API.Controllers
{
	[Route("api/v1.0/product")]
	[ApiController]
#if !DEBUG
	[Authorize]
#endif
	public class ProductController : ApiControllerBase
	{
		private readonly IProductQuery _productQuery;
		private readonly IProductService _productService;
		private readonly IObjectMapper _mapper;

		public ProductController(
			IProductQuery productQuery,
			IProductService productService,
			IObjectMapper mapper)
		{
			_productQuery = productQuery;
			_productService = productService;
			_mapper = mapper;
		}

		[HttpGet("q1")]
		public async Task<PagedResult<ProductOut>> PagedQuery1Async(string keyword, int page, int limit)
		{
			var @out = await _productQuery.PagedQueryAsync(keyword, page, limit);
			return _mapper.MapPagedResult<ProductOut>(@out);
		}

		[HttpGet("q2")]
		public async Task<Response<PagedResult<ProductOut>>> PagedQuery2Async(string keyword, int page, int limit)
		{
			var @out = await _productQuery.PagedQueryAsync(keyword, page, limit);
			return new Response<PagedResult<ProductOut>>(@out);
		}

		[HttpPost]
		public async Task<CreatProductOut> CreateAsync([FromBody] CreateProductViewObject vo)
		{
			var @in = _mapper.Map<CreateProductIn>(vo);
			var result = await _productService.CreateAsync(@in);
			return result;
		}

		[HttpGet("Error")]
		public Response GetErrorAsync()
		{
			return new ErrorResponse("I am an error response");
		}

		[HttpGet("MSFrameworkException")]
		public void GetFrameworkException()
		{
			throw new MSFrameworkException(2, "i'm framework exception");
		}

		[HttpGet("Exception")]
		public Response GetExceptionAsync()
		{
			throw new Exception("i'm framework exception");
		}

		[HttpGet("getByName")]
		public async Task<ProductOut> GetAsync(string name)
		{
			return await _productQuery.GetByNameAsync(name);
		}

		[HttpDelete]
		public async Task<ProductOut> DeleteAsync(Guid productId)
		{
			return await _productService.DeleteByIdAsync(productId);
		}
	}
}