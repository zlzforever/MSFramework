using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;
using MicroserviceFramework.AspNetCore;
using MicroserviceFramework.Domain;
using MicroserviceFramework.Ef.Repositories;
using MicroserviceFramework.Mediator;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using MongoDB.Bson;
using Ordering.Application.Commands;
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
    private readonly IMediator _cqrsProcessor;
    private readonly OrderingContext _dbContext;
    private readonly IUnitOfWork _unitOfWorkManager;
    private readonly IExternalEntityRepository _externalEntityRepository;

    public OrderController(IOrderingRepository orderRepository,
        IOrderingQuery orderingQuery, IMediator commandExecutor, OrderingContext dbContext,
        IUnitOfWork unitOfWorkManager, IExternalEntityRepository externalEntityRepository)
    {
        _orderingQuery = orderingQuery;
        _cqrsProcessor = commandExecutor;
        _dbContext = dbContext;
        _unitOfWorkManager = unitOfWorkManager;
        _externalEntityRepository = externalEntityRepository;
        _orderRepository = orderRepository;
    }

    [HttpPost("createTest")]
    public async Task<Order> TestCreate()
    {
        var order = Order.Create(
            "1",
            new Address("Street", "City", "State", "Country", "ZipCode"),
            "Description");
        order.AddItem("100049450275",
            "英特尔(Intel) i5-13400F 13代 酷睿 处理器 10核16线程 睿频至高可达4.6Ghz 20M三级缓存 台式机CPU", 149900, 0,
            "https://img10.360buyimg.com/n1/s450x450_jfs/t1/125974/18/29337/184045/63ae90f3F3d8b8b8a/b6cef93bb9b3b2c1.jpgl",
            1);
        order.AddItem("100041994142",
            "ROG ROG STRIX Z790-A GAMING WIFI吹雪主板 支持DDR5 CPU 13900K/13700K（Intel Z790/LGA 1700）", 28400, 0,
            "https://img12.360buyimg.com/n1/s450x450_jfs/t1/177676/26/33690/186079/63f71ca3F72878ea9/54e9c6c564a1d4e1.jpg",
            1);
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
        return order;
    }

    #region Command

    /// <summary>
    /// 测试有返回值的命令
    /// </summary>
    /// <param name="command"></param>
    /// <returns></returns>
    [HttpPost("tesCommand1")]
    public async Task<string> ExecuteTestCommand1Async([FromBody] TestCommand1 command)
    {
        var a = await _cqrsProcessor.SendAsync(command, default);
        return a;
    }

    /// <summary>
    /// 测试无返回值的命令
    /// </summary>
    /// <param name="command"></param>
    [HttpPost("tesCommand2")]
    public async Task ExecuteTestCommand2Async([FromBody] TestCommand2 command)
    {
        await _cqrsProcessor.SendAsync(command, default);
    }

    /// <summary>
    /// FOR TEST Method
    /// </summary>
    /// <returns></returns>
    [HttpPost]
    //[AccessControl("创建订单")]
    public async Task<ObjectId> CreateAsync([FromBody] CreateOrderCommand command)
    {
        return await _cqrsProcessor.SendAsync(command);
    }

    [HttpDelete("{orderId}")]
    public async Task DeleteAsync([FromRoute] DeleteOrderCommand command)
    {
        await _cqrsProcessor.SendAsync(command);
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
    public async Task<Order> GetAsync([FromRoute, Required] ObjectId orderId)
    {
        var order = await _orderingQuery.GetAsync(orderId);
        return order;
    }

    [HttpGet]
    public async Task<IEnumerable<Order>> GetAsync()
    {
        var order = await _dbContext.Set<Order>().FirstOrDefaultAsync();
        if (order == null)
        {
            return Enumerable.Empty<Order>();
        }

        order.AddEvent();
        await _unitOfWorkManager.SaveChangesAsync();
        var orders = await _dbContext.Set<Order>().ToListAsync();
        return orders;
    }

    #endregion
}
