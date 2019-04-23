using System;
using System.Collections.Generic;
using System.Net;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using MSFramework.Command;
using MSFramework.Reflection;
using MSFramework.Security;
using Ordering.API.Application.Command;
using Ordering.API.Application.DTO;
using Ordering.API.Application.Query;

namespace Ordering.API.Controllers
{
	[Route("api/v1/[controller]")]
	//[Authorize]
	[ApiController]
	public class OrderController : ControllerBase
	{
		private readonly ILogger _logger;
		private readonly ICurrentUser _currentUser;
		private readonly IOrderService _orderService;
		private readonly ICommandBus _commandBus;

		public OrderController(IOrderService orderService,
			ICommandBus commandBus,
			ICurrentUser currentUser, ILogger<OrderController> logger)
		{
			_orderService = orderService;
			_logger = logger;
			_currentUser = currentUser;
			_commandBus = commandBus;
		}

		[HttpPost("")]
		public async Task<IActionResult> CreateOrderAsync()
		{
			var random = new Random();
			var items = new List<OrderItemDTO>();
			var count = random.Next(2, 5);
			for (int i = 0; i < count; ++i)
			{
				var product = Guid.NewGuid();
				items.Add(new OrderItemDTO
				{
					ProductName = "product" + product.ToString("N"),
					ProductId = product,
					Units = random.Next(1, 10),
					Discount = 0,
					UnitPrice = random.Next(2, 1000)
				});
			}

			// FOR TEST
			return Ok(await _commandBus.SendAsync(new CreateOrderCommand(items,
				"HELLO",
				"上海", "张扬路500号", "上海", "中国", "200000", "what?")));
		}

		[HttpDelete("{orderId}")]
		public async Task<IActionResult> DeleteOrderAsync(Guid orderId, [FromQuery] long version)
		{
			return Ok(await _commandBus.SendAsync(new DeleteOrderCommand
			{
				OrderId = orderId,
				Version = version
			}));
		}

		[HttpPut("{orderId}/address")]
		public async Task<IActionResult> ChangeOrderAddressAsync(Guid orderId,
			[FromBody] ChangeOrderAddressCommand command)
		{
			command.OrderId = orderId;
			return Ok(await _commandBus.SendAsync(command));
		}

		#region QUERY

		[HttpGet("{orderId}")]
		public async Task<ActionResult> GetOrderAsync(string orderId)
		{
			var order = await _orderService.GetOrderAsync(Guid.NewGuid());
			return Ok(order);
		}

		#endregion
	}
}