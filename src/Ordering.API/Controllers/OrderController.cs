using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;
using Dapr;
using Dapr.Client;
using MicroserviceFramework;
using MicroserviceFramework.AspNetCore;
using MicroserviceFramework.Domain;
using MicroserviceFramework.Ef.Repositories;
using MicroserviceFramework.Mediator;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using MongoDB.Bson;
using Ordering.Application.Commands;
using Ordering.Application.Dto;
using Ordering.Application.Events;
using Ordering.Application.Queries;
using Ordering.Domain.AggregateRoots;
using Ordering.Domain.Repositories;
using Ordering.Infrastructure;

namespace Ordering.API.Controllers;

[Route("api/v1.0/[controller]")]
[ApiController]
public class OrderController : ApiControllerBase
{
    private readonly IOrderingQuery _orderingQuery;
    private readonly IOrderingRepository _orderRepository;
    private readonly IMediator _mediator;
    private readonly OrderingContext _dbContext;
    private readonly IUnitOfWork _unitOfWorkManager;
    private readonly IExternalEntityRepository<User, string> _externalEntityRepository;
    private readonly IObjectAssembler _objectAssembler;
    private readonly ILogger<OrderController> _logger;
    private readonly DaprClient _daprClient;

    public OrderController(IOrderingRepository orderRepository,
        IOrderingQuery orderingQuery, OrderingContext dbContext,
        IUnitOfWork unitOfWorkManager, IExternalEntityRepository<User, string> externalEntityRepository,
        IObjectAssembler objectAssembler, IMediator mediator, ILogger<OrderController> logger, DaprClient daprClient)
    {
        _orderingQuery = orderingQuery;
        _dbContext = dbContext;
        _unitOfWorkManager = unitOfWorkManager;
        _externalEntityRepository = externalEntityRepository;
        _objectAssembler = objectAssembler;
        _mediator = mediator;
        _logger = logger;
        _daprClient = daprClient;
        _orderRepository = orderRepository;
    }

    [HttpPost("createTest")]
    public async Task<OrderDto> TestCreate()
    {
        var order = Order.Create(
            "1",
            new Address("Street", "City", "State", "Country", "ZipCode"),
            "Description");
        order.AddItem("100049450275",
            "英特尔(Intel) i5-13400F 13代 酷睿 处理器 10核16线程 睿频至高可达4.6Ghz 20M三级缓存 台式机CPU",
            "https://img10.360buyimg.com/n1/s450x450_jfs/t1/125974/18/29337/184045/63ae90f3F3d8b8b8a/b6cef93bb9b3b2c1.jpgl",
            149900);
        order.AddItem("100041994142",
            "ROG ROG STRIX Z790-A GAMING WIFI吹雪主板 支持DDR5 CPU 13900K/13700K（Intel Z790/LGA 1700）",
            "https://img12.360buyimg.com/n1/s450x450_jfs/t1/177676/26/33690/186079/63f71ca3F72878ea9/54e9c6c564a1d4e1.jpg",
            28400);
        order.SetList(new[] { "hi1", "hi2" });
        order.AddExtra("质保", "3 年");
        order.AddExtra("RGB", "ARGB");
        order.AddKeyValue("test1", "value1");
        order.AddKeyValue("test2", "value2");

        // var user1 = _externalEntityRepository.GetOrCreate<User, string>(() => new User("1") { Name = "Lewis" });
        //i1.SetCreator(user1);

        // var user2 = _externalEntityRepository.GetOrCreate<User, string>(() => new User("1"));
        //i2.SetCreator(user2);

        // var user3 = _externalEntityRepository.GetOrCreate<User, string>(() => new User("1"));
        // order.SetCreator(user3);
        await _orderRepository.AddAsync(order);
        Logger.LogInformation("{TraceIdentifier}: Create test order completed", Session.TraceIdentifier);
        return _objectAssembler.To<OrderDto>(order);
    }

    [Topic("pubsub", Names.OrderCreatedEvent)]
    [HttpPost("OnProductCreated")]
    public Task OnProductCreatedAsync(E e)
    {
        var a = Defaults.JsonSerializer.Serialize(e);
        _logger.LogInformation(a);
        return Task.CompletedTask;
    }

    public class E
    {
        public string Id { get; set; }
        public string Name { get; set; }
        public DateTimeOffset CreationTime { get; set; }
    }

    #region Command

    /// <summary>
    /// FOR TEST Method
    /// </summary>
    /// <returns></returns>
    [HttpPost]
    //[AccessControl("创建订单")]
    public async Task<ObjectId> CreateAsync([FromBody] CreateOrderCommand command)
    {
        return await _mediator.SendAsync(command);
    }

    [HttpDelete("{orderId}")]
    public async Task DeleteAsync([FromRoute] DeleteOrderCommand command)
    {
        await _mediator.SendAsync(command);
    }

    [HttpPut("{orderId}/address")]
    public Task ChangeAddressAsync([FromRoute] ObjectId orderId,
        [FromBody] ChangeOrderAddressCommand command)
    {
        command.OrderId = orderId;
        return Task.CompletedTask;
    }

    #endregion

    #region QUERY

    [HttpGet("{orderId}")]
    public async Task<OrderDto> GetAsync([FromRoute, Required] ObjectId orderId)
    {
        var order = await _orderingQuery.GetAsync(orderId);
        return _objectAssembler.To<OrderDto>(order);
    }

    [HttpGet]
    public async Task<IEnumerable<OrderDto>> GetAsync()
    {
        var order = await _dbContext.Set<Order>().FirstOrDefaultAsync();
        if (order == null)
        {
            return Enumerable.Empty<OrderDto>();
        }

        order.AddEvent();
        await _unitOfWorkManager.SaveChangesAsync();
        var orders = await _dbContext.Set<Order>().ToListAsync();
        return orders.Select(x => _objectAssembler.To<OrderDto>(x));
    }

    #endregion
}
