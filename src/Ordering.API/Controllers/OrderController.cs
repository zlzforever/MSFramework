using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;
using Dapr;
using MicroserviceFramework.AspNetCore;
using MicroserviceFramework.Domain;
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
    IExternalEntityRepository<UserInfo, string> externalEntityRepository,
    IObjectAssembler objectAssembler,
    IMediator mediator,
    ILogger<OrderController> logger,
    IJsonSerializer jsonSerializer)
    : ApiControllerBase
{
    [HttpPost("createTest")]
    public async Task<(OrderDto Order1, OrderDto Order2, OrderDto Order3)> CreateTest(bool single = false)
    {
        var user1 = externalEntityRepository.Load(UserInfo.Create("1", "Lewis"));
        var order1 = await AddAsync(user1);
        await unitOfWork.SaveChangesAsync();
        if (!single)
        {
            var user2 = externalEntityRepository.Load(UserInfo.Create("1", "Lewis"));
            var order2 = await AddAsync(user2);
            var order3 = await AddAsync(user2);
            Logger.LogInformation("{TraceIdentifier}: Create test order completed", Session.TraceIdentifier);
            return (objectAssembler.To<OrderDto>(order1), objectAssembler.To<OrderDto>(order2),
                objectAssembler.To<OrderDto>(order3));
        }

        Logger.LogInformation("{TraceIdentifier}: Create test order completed", Session.TraceIdentifier);
        return (objectAssembler.To<OrderDto>(order1), null, null);
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
    public async Task<string> Create([FromBody] CreateOrderCommand command)
    {
        return await mediator.SendAsync(command);
    }

    [HttpDelete("{orderId}")]
    public async Task Delete([FromRoute] DeleteOrderCommand command)
    {
        await mediator.SendAsync(command);
    }

    [HttpPut("{orderId}/address")]
    public async Task SetAddress([FromRoute, StringLength(36), Required] string orderId,
        [FromBody] SetOrderAddressCommand command)
    {
        command.OrderId = orderId;
        await mediator.SendAsync(command);
    }

    #endregion

    #region QUERY

    [HttpGet("{orderId}")]
    public async Task<OrderDto> GetAsync([FromRoute, Required, StringLength(36)] string orderId)
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
        var orders = await orderingQuery.GetAllListAsync();
        return orders.Select(objectAssembler.To<OrderDto>);
    }

    #endregion

    private async Task<Order> AddAsync(UserInfo user1)
    {
        var address = new Address
        {
            Street = "Street",
            City = "City",
            State = "State",
            Country = "Country",
            ZipCode = "ZipCode"
        };

        var order = Order.Create(
            user1,
            address,
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
        order.AddKeyValuePair("test1", "value1");
        order.AddKeyValuePair("test2", "value2");

        logger.LogInformation("{TraceIdentifier}: Create test order", Session.TraceIdentifier);

        await orderRepository.AddAsync(order);
        return order;
    }
}
