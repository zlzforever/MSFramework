using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Threading.Tasks;
using Dapr;
using Dapr.Client;
using MicroserviceFramework;
using MicroserviceFramework.AspNetCore;
using MicroserviceFramework.AspNetCore.Mvc;
using MicroserviceFramework.Common;
using MicroserviceFramework.Domain;
using Microsoft.AspNetCore.Mvc;
using MongoDB.Bson;
using Ordering.Domain.AggregateRoots;
using Ordering.Domain.Repositories;
using MicroserviceFramework.AspNetCore.Extensions;
using Microsoft.AspNetCore.Http;
using Ordering.Application.EventHandlers;

namespace Ordering.API.Controllers;

public class CreateViewObject
{
    /// <summary>
    /// 
    /// </summary>
    [Required]
    [StringLength(50)]
    public string Name { get; set; }
}

public class ProductDTO
{
    public string Name { get; private set; }

    public int Price { get; private set; }
}

public class MyBody
{
    public ObjectId Id { get; set; }
    public ObjectId MyId { get; set; }
    public string Name { get; set; }
}

[Route("api/v1.0/[controller]")]
[ApiController]
public class ProductController : ApiControllerBase
{
    private readonly IProductRepository _productRepository;
    private readonly IObjectAssembler _mapper;
    private readonly IUnitOfWork _unitOfWork;

    public ProductController(IProductRepository productRepository,
        IObjectAssembler mapper, IUnitOfWork unitOfWork)
    {
        _productRepository = productRepository;
        _mapper = mapper;
        _unitOfWork = unitOfWork;
    }

    [HttpGet("objectid")]
    public ObjectId Get()
    {
        return ObjectId.GenerateNewId();
    }

    [HttpPost("objectid/{id}")]
    public MyBody Post([FromRoute] ObjectId id, [FromBody] MyBody body)
    {
        body.Id = id;
        return body;
    }


    [HttpGet("BaseValueType")]
    public int GetBaseValueType()
    {
        return 1;
    }

    [HttpGet("First1")]
    //[AccessControl("查看第一个产品", "产品")]
    public Product GetFirst1()
    {
        return _productRepository.GetFirst();
    }

    [HttpGet("First2")]
    public Product GetFirst2()
    {
        var a = _productRepository.GetFirst();
        return a;
    }

    [HttpGet("PagedQuery")]
    //[AccessControl("查询产品", "产品")]
    public async Task<PagedResult<ProductDTO>> GetPagedQuery()
    {
        var a = await _productRepository.PagedQueryAsync(0, 10);
        var b = new PagedResult<ProductDTO>(a.Page, a.Limit, a.Total, _mapper.To<List<ProductDTO>>(a.Data));
        return b;
    }

    [HttpGet("Error")]
    public int GetErrorAsync()
    {
        HttpContext.WriteError(1, "I am an error");
        return -1;
    }

    [HttpGet("FrameworkException")]
    public int GetMicroserviceFrameworkException()
    {
        throw new MicroserviceFrameworkException(2, "i'm a framework exception");
    }

    [HttpGet("FrameworkFriendlyException")]
    public int GetMicroserviceFrameworkFriendlyException()
    {
        throw new MicroserviceFrameworkFriendlyException(2, "i'm a framework friendly exception");
    }

    [HttpGet("Exception")]
    public int GetExceptionAsync()
    {
        throw new Exception("i'm an exception");
    }


    [HttpGet("NoResponse")]
    public Task NoResponse()
    {
        return Task.CompletedTask;
    }

    [HttpGet("File")]
    public FileResult GetFile()
    {
        return PhysicalFile("1.csv", "application/txt");
    }

    [HttpGet]
    public Product GetAsync(ObjectId productId)
    {
        return _productRepository.Find(productId);
    }

    [HttpDelete]
    //[AccessControl("删除产品", "产品")]
    public Product DeleteAsync(ObjectId productId)
    {
        var product = _productRepository.Find(productId);
        _productRepository.Delete(product);
        return product;
    }

    [HttpPost]
    public async Task<Product> CreateAsync(CreateViewObject vo)
    {
        var prod = Product.Create(vo.Name, new Random().Next(100, 10000));
        await _productRepository.AddAsync(prod);
        await _unitOfWork.SaveChangesAsync();
        return prod;
    }

    [Topic("pubsub", "ProjectCreatedIntegrationEvent")]
    [HttpPost("Created")]
    public async Task CreatedAsync(ProjectCreatedIntegrationEvent @event)
    {
        var product = await _productRepository.FindAsync(@event.Id);
        if (product != null)
        {
            product.SetName(Guid.NewGuid().ToString());
            await _unitOfWork.CommitAsync();
        }
    }
}
