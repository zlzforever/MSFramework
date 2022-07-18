﻿using System;
using System.ComponentModel.DataAnnotations;
using System.Threading.Tasks;
using MicroserviceFramework;
using MicroserviceFramework.AspNetCore;
using MicroserviceFramework.AspNetCore.Mvc;
using MicroserviceFramework.Common;
using MicroserviceFramework.Domain;
using Microsoft.AspNetCore.Mvc;
using MongoDB.Bson;
using Ordering.Domain.AggregateRoots;
using Ordering.Domain.Repositories;

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
    internal ObjectId Id { get; set; }
    public ObjectId MyId { get; set; }
    public string Name { get; set; }
}

[Route("api/v1.0/[controller]")]
[ApiController]
public class ProductController : ApiControllerBase
{
    private readonly IProductRepository _productRepository;
    private readonly IObjectAssembler _mapper;

    public ProductController(IProductRepository productRepository,
        IObjectAssembler mapper)
    {
        _productRepository = productRepository;
        _mapper = mapper;
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


    [HttpGet("getBaseValueType")]
    public int GetBaseValueType()
    {
        return 1;
    }

    [HttpGet("getFirst1")]
    //[AccessControl("查看第一个产品", "产品")]
    public Product GetFirst1()
    {
        return _productRepository.GetFirst();
    }

    [HttpGet("getFirst2")]
    public ApiResult<Product> GetFirst2()
    {
        var a = _productRepository.GetFirst();
        return new ApiResult<Product>(a);
    }

    [HttpGet("getPagedQuery")]
    //[AccessControl("查询产品", "产品")]
    public async Task<ApiResult<PagedResult<ProductDTO>>> GetPagedQuery()
    {
        var a = await _productRepository.PagedQueryAsync(0, 10);
        var b = _mapper.To<PagedResult<ProductDTO>>(a);
        return new ApiResult<PagedResult<ProductDTO>>(b);
    }

    [HttpGet("getError")]
    public ApiResult GetErrorAsync()
    {
        return Error("I am an error response");
    }

    [HttpGet("getMSFrameworkException")]
    public ApiResult GetMSFrameworkException()
    {
        throw new MicroserviceFrameworkException(2, "i'm framework exception");
    }

    [HttpGet("getException")]
    public ApiResult GetExceptionAsync()
    {
        throw new Exception("i'm framework exception");
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
    public Product CreateAsync(CreateViewObject vo)
    {
        var prod = Product.Create(vo.Name, new Random().Next(100, 10000));
        _productRepository.Add(prod);
        return prod;
    }
}
