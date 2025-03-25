using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;
using MicroserviceFramework;
using MicroserviceFramework.AspNetCore;
using MicroserviceFramework.AspNetCore.Extensions;
using MicroserviceFramework.Common;
using MicroserviceFramework.Domain;
using MicroserviceFramework.Linq.Expression;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using MongoDB.Bson;
using Ordering.Domain.AggregateRoots;
using Ordering.Domain.Repositories;
using Ordering.Infrastructure;


namespace Ordering.API.Controllers;

[Route("api/v1.0/[controller]")]
[ApiController]
public class ProductController(
    IProductRepository productRepository,
    IObjectAssembler mapper,
    IUnitOfWork unitOfWork,
    OrderingContext orderingContext,
    ILogger<ProductController> logger)
    : ApiControllerBase
{
    [HttpGet("id")]
    public ObjectId Get()
    {
        return ObjectId.GenerateNewId();
    }

    [HttpGet("modalState")]
    public bool ModalState([Required] string id)
    {
        return true;
    }

    [HttpPost("file")]
    public async Task<string> Upload()
    {
        var file = HttpContext.Request.Form.Files.First();
        var tuple = await file.SaveAsync();
        return tuple.Path;
    }

    [HttpPost("objectid/{id}")]
    public MyBody Post([FromRoute] string id, [FromBody] MyBody body)
    {
        body.Id = id;
        return body;
    }

    [HttpGet("BaseValueType")]
    public int GetBaseValueType()
    {
        return 1;
    }

    [HttpGet("First")]
    //[AccessControl("查看第一个产品", "产品")]
    public ProductOut GetFirst()
    {
        var queryable = from p in orderingContext.Set<Product>()
            join u in orderingContext.Set<User>() on p.CreatorId equals Convert.ToString(u.Id)
            select new ProductOut { Name = p.Name, Price = p.Price, CreatorName = u.Name };
        return queryable.FirstOrDefault();
    }


    [HttpGet("PagedQuery")]
    //[AccessControl("查询产品", "产品")]
    public async Task<PaginationResult<ProductDTO>> GetPagedQuery()
    {
        var a = await orderingContext.Set<Product>().PagedQueryAsync(0, 10);
        var b = new PaginationResult<ProductDTO>(a.Page, a.Limit, a.Total, mapper.To<List<ProductDTO>>(a.Data));
        return b;
    }

    [HttpGet("Error")]
    public int GetErrorAsync()
    {
        logger.LogError(
            "Test {String} {Bool} {Int} {Double} {Byte} {Decimal} {Float} {Short} {Long} {DateTime} {Guid}",
            "String", true, 1, 1.1d, 1, 1.1m, 1.1f, 1, 1, DateTime.Now, Guid.NewGuid());
        throw new Exception("I am an error");
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

    [HttpGet("type")]
    public string GetType([FromQuery] ProductType type)
    {
        return type.Id;
    }

    [HttpGet("NoResponse")]
    public Task NoResponse()
    {
        return Task.CompletedTask;
    }

    [HttpGet("File")]
    public FileResult GetFile()
    {
        return PhysicalFile(AppContext.BaseDirectory + "1.csv", "application/txt");
    }

    [HttpGet]
    public Product GetAsync(string productId)
    {
        return productRepository.Find(productId);
    }

    [HttpDelete]
    //[AccessControl("删除产品", "产品")]
    public Product DeleteAsync(string productId)
    {
        var product = productRepository.Find(productId);
        productRepository.Delete(product);
        return product;
    }

    [HttpPost]
    public async Task<Product> CreateAsync(CreateViewObject vo)
    {
        var prod = Product.Create(vo.Name, new Random().Next(100, 10000));
        prod.SetCreation("1", "1");
        await productRepository.AddAsync(prod);
        await unitOfWork.SaveChangesAsync();

        var prod2 = Product.Create(vo.Name, new Random().Next(100, 10000));
        prod2.SetCreation("1", "1");
        await productRepository.AddAsync(prod2);
        await unitOfWork.SaveChangesAsync();
        return prod;
    }

    // [HttpPost("CAP")]
    // [CapTransaction]
    // public async Task<IActionResult> EntityFrameworkWithTransaction([FromServices] OrderingContext dbContext)
    // {
    //     _logger.LogInformation("调用开始");
    //     var prod = Product.Create("CAP", new Random().Next(100, 10000));
    //     prod.SetCreation("1", "1");
    //     await dbContext.AddAsync(prod);
    //
    //     _logger.LogInformation("调用结束");
    //
    //     return Ok();
    // }

    // [CapSubscribe("subscribe")]
    // [NonAction]
    // public void Subscribe()
    // {
    // }
    //
    // [HttpGet("CAP_Exception")]
    // [CapTransaction]
    // public IActionResult EntityFrameworkWithTransactionException([FromServices] OrderingContext dbContext)
    // {
    //     throw new MicroserviceFrameworkFriendlyException("发生异常");
    // }
    //
    // [HttpPost("CAPFail")]
    // [CapTransaction]
    // public async Task<IActionResult> EntityFrameworkWithTransactionFail([FromServices] OrderingContext dbContext)
    // {
    //     var prod = Product.CreateWithoutEvent("CAP", new Random().Next(100, 10000));
    //     prod.SetCreation("1", "1");
    //
    //     await dbContext.AddAsync(prod);
    //
    //     await _capBus.PublishAsync(Names.ProjectCreateFailedEvent,
    //         new { prod.Id, prod.Name, CreationTime = DateTimeOffset.Now });
    //
    //     await dbContext.SaveChangesAsync();
    //     return Ok();
    // }

    [HttpGet("testPaged")]
    public PaginationResult<TestData> TestPagedResult()
    {
        return new PaginationResult<TestData>(0, 1, 10,
            [new TestData { Id = "1", Name = "A1" }, new TestData { Id = "2", Name = "A2" }]);
    }
}

public class TestData
{
    public string Id { get; set; }
    public string Name { get; set; }
}

public class ProductOut
{
    public string Name { get; set; }

    public int Price { get; set; }


    public string CreatorName { get; set; }
}

public class CreateViewObject
{
    /// <summary>
    ///
    /// </summary>
    [Required]
    [StringLength(50)]
    public string Name { get; set; }

    public ProductType Type { get; set; }
}

public class ProductDTO
{
    public string Name { get; private set; }

    public int Price { get; private set; }
}

public class MyBody
{
    public string Id { get; set; }
    public string MyId { get; set; }
    public string Name { get; set; }
}
