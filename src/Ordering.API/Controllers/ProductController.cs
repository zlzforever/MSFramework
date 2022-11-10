using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;
using Dapr;
using Dapr.Client;
using DotNetCore.CAP;
using MicroserviceFramework;
using MicroserviceFramework.AspNetCore;
using MicroserviceFramework.AspNetCore.Extensions;
using MicroserviceFramework.Common;
using MicroserviceFramework.Domain;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using MongoDB.Bson;
using Ordering.Domain.AggregateRoots;
using Ordering.Domain.Repositories;
using Ordering.Application.EventHandlers;
using Ordering.Infrastructure;

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
    private readonly ICapPublisher _capBus;
    private readonly OrderingContext _orderingContext;

    public ProductController(IProductRepository productRepository,
        IObjectAssembler mapper, IUnitOfWork unitOfWork, ICapPublisher capBus, OrderingContext orderingContext)
    {
        _productRepository = productRepository;
        _mapper = mapper;
        _unitOfWork = unitOfWork;
        _capBus = capBus;
        _orderingContext = orderingContext;
    }

    [HttpGet("objectid")]
    public ObjectId Get()
    {
        return ObjectId.GenerateNewId();
    }

    [HttpGet("modalState")]
    public bool ModalState([Required] string id)
    {
        return true;
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

    [HttpGet("First")]
    //[AccessControl("查看第一个产品", "产品")]
    public ProductOut GetFirst()
    {
        var queryable = from p in _orderingContext.Set<Product>()
            join u in _orderingContext.Set<User>() on p.CreatorId equals u.Id
            select new ProductOut { Name = p.Name, Price = p.Price, CreatorName = u.Name };
        return queryable.FirstOrDefault();
    }

    public class ProductOut
    {
        public string Name { get; set; }

        public int Price { get; set; }

        public string CreatorName { get; set; }
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
        prod.SetCreation("1");
        await _productRepository.AddAsync(prod);
        // await _unitOfWork.SaveChangesAsync();
        return prod;
    }

    [Topic("pubsub", "Ordering.Application.EventHandlers.ProjectCreatedIntegrationEvent")]
    [HttpPost("Created")]
    public async Task CreatedAsync([FromBody] ProjectCreatedIntegrationEvent @event)
    {
        var ip = HttpContext.GetRemoteIpAddress();
        var product = await _productRepository.FindAsync(@event.Id);
        if (product != null)
        {
            product.SetName(Guid.NewGuid().ToString());
            // await _unitOfWork.CommitAsync();
        }

        Logger.LogInformation($"{ip}: Created");
    }

    [HttpPost("CAP")]
    public IActionResult EntityFrameworkWithTransaction([FromServices] OrderingContext dbContext)
    {
        using (var trans = dbContext.Database.BeginTransaction(_capBus, autoCommit: true))
        {
            //your business logic code

            _capBus.Publish("Ordering.Application.EventHandlers.ProjectCreatedIntegrationEvent", DateTime.Now);
        }

        DaprClient client;

        return Ok();
    }

    [CapSubscribe("Ordering.Application.EventHandlers.ProjectCreatedIntegrationEvent")]
    [NonAction]
    public void CheckReceivedMessage(DateTime datetime)
    {
        Console.WriteLine(datetime);
    }
}
