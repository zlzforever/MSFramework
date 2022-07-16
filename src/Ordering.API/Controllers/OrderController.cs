using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Threading.Tasks;
using MicroserviceFramework.AspNetCore;
using MicroserviceFramework.Domain;
using MicroserviceFramework.Mediator;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using MongoDB.Bson;
using Ordering.Application.Commands;
using Ordering.Application.Queries;
using Ordering.Domain.AggregateRoots;
using Ordering.Domain.Repositories;
using Ordering.Infrastructure;

namespace Ordering.API.Controllers
{
    [Route("api/v1.0/[controller]")]
    [ApiController]
    public class OrderController : ApiControllerBase
    {
        private readonly IOrderingQuery _orderingQuery;
        private readonly IOrderingRepository _orderRepository;
        private readonly IMediator _cqrsProcessor;
        private readonly OrderingContext _dbContext;
        private readonly IUnitOfWork _unitOfWorkManager;

        public OrderController(IOrderingRepository orderRepository,
            IOrderingQuery orderingQuery, IMediator commandExecutor, OrderingContext dbContext,
            IUnitOfWork unitOfWorkManager)
        {
            _orderingQuery = orderingQuery;
            _cqrsProcessor = commandExecutor;
            _dbContext = dbContext;
            _unitOfWorkManager = unitOfWorkManager;
            _orderRepository = orderRepository;
        }

        //[AccessControl("TestCreate")]
        [Route("createTest")]
        [HttpPost]
        public async Task<IActionResult> TestCreate()
        {
            var order = Order.Create(
                "testUSer",
                new Address("Street", "City", "State", "Country", "ZipCode"),
                "Description");
            order.AddItem(Guid.NewGuid(),
                "testProduct1", 10, 0, "");
            order.AddItem(Guid.NewGuid(),
                "testProduct2", 10, 0, "");
            order.SetRivalNetwork(new[] { "hi1", "hi2" });
            order.AddKeyValue("test1", "value1");
            order.AddKeyValue("test2", "value2");
            order.AddExtra("n1", "a1");
            order.AddExtra("n2", "a2");
            await _orderRepository.AddAsync(order);
            return Ok(order);
        }

        #region Command

        [HttpPost("test-command1")]
        public async Task<string> TestCommand1Async([FromBody] TestCommand1 command)
        {
            var a = await _cqrsProcessor.SendAsync(command, default);
            return a;
        }

        [HttpPost("test-command2")]
        //[AccessControl("test-command2")]
        public async Task TestCommand2Async([FromBody] TestCommand2 command)
        {
            await _cqrsProcessor.SendAsync(command, default);
        }

        /// <summary>
        /// FOR TEST Method
        /// </summary>
        /// <returns></returns>
        [HttpPost]
        //[AccessControl("创建订单")]
        public async Task<ObjectId> CreateOrderAsync()
        {
            var random = new Random();
            var items = new List<CreateOrderCommand.OrderItemDTO>();
            var count = random.Next(2, 5);
            for (var i = 0; i < count; ++i)
            {
                var product = Guid.NewGuid();
                items.Add(new CreateOrderCommand.OrderItemDTO
                {
                    ProductName = "product" + product.ToString("N"),
                    ProductId = product,
                    Units = random.Next(1, 10),
                    Discount = 0,
                    UnitPrice = random.Next(2, 1000)
                });
            }

            return await _cqrsProcessor.SendAsync(new CreateOrderCommand(items, "HELLO", "上海", "张扬路500号", "上海",
                "中国", "200000", "what?"));
        }

        [HttpDelete("{orderId}")]
        //[AccessControl("删除订单")]
        public async Task<IActionResult> DeleteOrderAsync([FromRoute] ObjectId orderId)
        {
            await _cqrsProcessor.SendAsync(new DeleteOrderCommand(orderId));
            return Ok();
        }

        [HttpPut("{orderId}/address")]
        //[AccessControl("修改订单地址")]
        public IActionResult ChangeOrderAddressAsync([FromRoute] ObjectId orderId,
            [FromBody] ChangeOrderAddressCommand command)
        {
            command.OrderId = orderId;
            return Ok();
        }

        #endregion

        #region QUERY

        [HttpGet("{orderId}")]
        //[AccessControl("查看订单")]
        public async Task<IActionResult> GetOrderAsync([FromRoute, Required] ObjectId orderId)
        {
            var order = await _orderingQuery.GetAsync(orderId);
            return Ok(order);
        }

        [HttpGet]
        // [AccessControl("查看所有订单")]
        public async Task<IEnumerable<Order>> GetOrdersAsync()
        {
            var order = await _dbContext.Set<Order>().FirstAsync();
            order.AddEvent();
            await _unitOfWorkManager.CommitAsync();
            return await _dbContext.Set<Order>().AsNoTracking().ToListAsync();
        }

        #endregion
    }
}