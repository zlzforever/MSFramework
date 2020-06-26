using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using MSFramework.AspNetCore;
using Ordering.Application.Command;
using Ordering.Application.Query;
using Ordering.Domain.AggregateRoot;
using Ordering.Domain.Repository;


namespace Ordering.API.Controllers
{
	[Route("api/v1.0/[controller]")]
	[ApiController]
	public class OrderController : ApiController
	{
		private readonly IOrderingQuery _orderingQuery;
		private readonly IOrderingRepository _orderRepository;

		public OrderController(IOrderingRepository orderRepository,
			IOrderingQuery orderingQuery)
		{
			_orderingQuery = orderingQuery;
			_orderRepository = orderRepository;
		}

		//[Permission(Name = "test", Module = "test222")]
		[HttpPost("testCreate")]
		public async Task<IActionResult> TestCreate()
		{
			var order = new Order(
				"testUSer",
				new Address("Street", "City", "State", "Country", "ZipCode"),
				"Description",
				new List<OrderItem>
				{
					new OrderItem(Guid.NewGuid(),
						"testProduct", 10, 0, "")
				});
			await _orderRepository.InsertAsync(order);
			// var order = await _orderRepository.GetAsync(Guid.Parse("35a00497-cbb0-4311-af5d-ab6b01281569"));
			// order.AddOrderItem(Guid.NewGuid(),
			// 	"testProduct", 10, 0, "");
			// await _orderRepository.UpdateAsync(order);
			return Ok(order);
		}

		#region Command

		/// <summary>
		/// FOR TEST Method
		/// </summary>
		/// <returns></returns>
		[HttpPost]
		public IActionResult CreateOrderAsync()
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

			return Ok();
			// return await _mediator.Send(new CreateOrderCommand(items, "HELLO",
			// 	"上海", "张扬路500号", "上海", "中国", "200000", "what?"));
		}

		[HttpDelete("{orderId}")]
		public IActionResult DeleteOrderAsync(Guid orderId)
		{
			// return await _mediator.Send(new DeleteOrderCommand(orderId));
			return Ok();
		}

		[HttpPut("{orderId}/address")]
		public IActionResult ChangeOrderAddressAsync(Guid orderId,
			[FromBody] ChangeOrderAddressCommand command)
		{
			command.OrderId = orderId;
			return Ok();
		}

		#endregion

		#region QUERY

		[HttpGet("{orderId}")]
		public async Task<IActionResult> GetOrderAsync(Guid orderId)
		{
			var order = await _orderingQuery.GetOrderAsync(orderId);
			return Ok(order);
		}

		[HttpGet()]
		public async Task<IActionResult> GetOrdersAsync()
		{
			var order = await _orderingQuery.GetAllOrdersAsync();
			return Ok(order);
		}

		#endregion
	}
}