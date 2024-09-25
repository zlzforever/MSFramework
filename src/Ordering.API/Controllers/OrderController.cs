using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;
using Dapr;
using MicroserviceFramework.AspNetCore;
using MicroserviceFramework.Domain;
using MicroserviceFramework.Ef.Repositories;
using MicroserviceFramework.Mediator;
using MicroserviceFramework.Serialization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Ordering.Application.Commands;
using Ordering.Application.Dto;
using Ordering.Application.Events;
using Ordering.Application.Queries;
using Ordering.Domain.AggregateRoots;
using Ordering.Domain.AggregateRoots.Order;
using Ordering.Domain.Repositories.Order;
using Ordering.Infrastructure;

namespace Ordering.API.Controllers;

[Route("api/v1.0/orders")]
[ApiController]
public class OrderController(
    IOrderRepository orderRepository,
    IOrderingQuery orderingQuery,
    OrderingContext dbContext,
    IUnitOfWork unitOfWork,
    IExternalEntityRepository<User, int> externalEntityRepository,
    IObjectAssembler objectAssembler,
    IMediator mediator,
    ILogger<OrderController> logger,
    IJsonSerializer jsonSerializer)
    : ApiControllerBase
{
    [HttpPost("createTest")]
    public async Task<(OrderDto Order1, OrderDto Order2)> TestCreate()
    {
        var order1 = await AddAsync();
        await unitOfWork.SaveChangesAsync();
        var order2 = await AddAsync();
        Logger.LogInformation("{TraceIdentifier}: Create test order completed", Session.TraceIdentifier);
        return (objectAssembler.To<OrderDto>(order1), objectAssembler.To<OrderDto>(order2));
    }

    private async Task<Order> AddAsync()
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

        var user1 = externalEntityRepository.GetOrCreate(() => new User(1) { Name = "Lewis" });
        order.SetOperator(user1);

        logger.LogInformation("{TraceIdentifier}: Create test order", Session.TraceIdentifier);

        await orderRepository.AddAsync(order);
        return order;
    }

    [Topic("rabbitmq-pubsub", Names.OrderCreatedEvent, "biz.ordering.dead-letter", false)]
    [HttpPost("OnOrderCreated")]
    public Task<bool> OnOrderCreatedAsync([FromBody] E e)
    {
        var a = jsonSerializer.Serialize(e);
        Console.WriteLine(a);
        logger.LogInformation(a);
        return Task.FromResult(true);
    }

    public class E
    {
        public string Id { get; set; }
        public string Name { get; set; }
        public string CreationTime { get; set; }
    }

    #region Command

    /// <summary>
    /// FOR TEST Method
    /// </summary>
    /// <returns></returns>
    [HttpPost]
    //[AccessControl("创建订单")]
    public async Task<string> CreateAsync([FromBody] CreateOrderCommand command)
    {
        return await mediator.SendAsync(command);
    }

    [HttpDelete("{orderId}")]
    public async Task DeleteAsync([FromRoute] DeleteOrderCommand command)
    {
        await mediator.SendAsync(command);
    }

    [HttpPut("{orderId}/address")]
    public Task ChangeAddressAsync([FromRoute] string orderId,
        [FromBody] ChangeOrderAddressCommand command)
    {
        command.OrderId = orderId;
        return Task.CompletedTask;
    }

    #endregion

    #region QUERY

    [HttpGet("{orderId}")]
    public async Task<OrderDto> GetAsync([FromRoute, Required] string orderId)
    {
        var order = await orderingQuery.GetAsync(orderId);
        return objectAssembler.To<OrderDto>(order);
    }

    [HttpGet]
    public async Task<IEnumerable<OrderDto>> GetAsync()
    {
        var order = await dbContext.Set<Order>().FirstOrDefaultAsync();
        if (order == null)
        {
            return Enumerable.Empty<OrderDto>();
        }

        order.AddEvent();
        await unitOfWork.SaveChangesAsync();
        var orders = await dbContext.Set<Order>().ToListAsync();
        return orders.Select(x => objectAssembler.To<OrderDto>(x));
    }

    #endregion
}
