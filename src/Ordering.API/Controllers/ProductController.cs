using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using MSFramework;
using MSFramework.AspNetCore;
using MSFramework.AspNetCore.Api;
using MSFramework.Audit;
using MSFramework.Data;
using MSFramework.Domain;
using MSFramework.Ef;
using MSFramework.Ef.Repository;
using Ordering.Domain.AggregateRoot;
using Ordering.Domain.Repository;

namespace Ordering.API.Controllers
{
	public class CreateViewObject
	{
		/// <summary>
		/// 
		/// </summary>
		[Required]
		[StringLength(50)]
		public string Name { get; set; }
	}


	[Route("api/v1.0/[controller]")]
	[ApiController]
	public class ProductController : ApiController
	{
		private readonly IProductRepository _productRepository;
		private readonly IRepository<AuditedOperation> _repository;

		public ProductController(IProductRepository productRepository, IRepository<AuditedOperation> repository)
		{
			_productRepository = productRepository;
			_repository = repository;
		}

		[HttpGet("getAudits")]
		public List<AuditedOperation> GetAudits()
		{
			Logger.LogInformation($"{Session.UserId}");
			return ((EfRepository<AuditedOperation>) _repository).CurrentSet.Include(x => x.Entities).ToList();
		}

		[HttpGet("getBaseValueType")]
		public int GetBaseValueType()
		{
			return 1;
		}

		[HttpGet("getFirst1")]
		public Product GetFirst1()
		{
			return _productRepository.GetFirst();
		}

		[HttpGet("getFirst2")]
		public Response<Product> GetFirst2()
		{
			var a = _productRepository.GetFirst();
			return new Response<Product>(a);
		}

		[HttpGet("getPagedQuery")]
		public async Task<Response<PagedResult<Product>>> GetPagedQuery()
		{
			var a = await _productRepository.PagedQueryAsync(0, 10);
			return new Response<PagedResult<Product>>(a);
		}

		[HttpGet("getError")]
		public Response GetErrorAsync()
		{
			return new ErrorResponse("I am an error response");
		}

		[HttpGet("getMSFrameworkException")]
		public Response GetMSFrameworkException()
		{
			throw new MSFrameworkException(2, "i'm framework exception");
		}

		[HttpGet("getException")]
		public Response GetExceptionAsync()
		{
			throw new Exception("i'm framework exception");
		}

		[HttpGet]
		public Product GetAsync(Guid productId)
		{
			return _productRepository.Get(productId);
		}

		[HttpDelete]
		public Product DeleteAsync(Guid productId)
		{
			return _productRepository.Delete(productId);
		}

		[HttpPost]
		public Product CreateAsync(CreateViewObject vo)
		{
			return _productRepository.Insert(new Product(vo.Name, new Random().Next(100, 10000)));
		}
	}
}