using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using MSFramework.Domain;
using Ordering.API.Application.DTO;
using Ordering.API.Application.Services;

namespace Ordering.API.Controllers
{
	[Route("api/v1/[controller]")]
	//[Authorize]
	[ApiController]
	public class OrderController : MSFrameworkControllerBase
	{
		private readonly ILogger _logger;
		private readonly IOrderingAppService _orderingAppService;

		public OrderController( 
			IOrderingAppService orderingAppService,
			IMSFrameworkSession session, ILogger<OrderController> logger) : base(session)
		{
			_logger = logger;
		 
			_orderingAppService = orderingAppService;
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

			await _orderingAppService.CreateOrder(new CreateOrderDTO(items,
				"HELLO",
				"上海", "张扬路500号", "上海", "中国", "200000", "what?"));
			// FOR TEST
			return Ok();
		}

		[HttpDelete("{orderId}")]
		public async Task<IActionResult> DeleteOrderAsync(Guid orderId)
		{
			await _orderingAppService.DeleteOrder(new DeleteOrderDto
			{
				OrderId = orderId
			});
			return Ok();
		}

		[HttpPut("{orderId}/address")]
		public async Task<IActionResult> ChangeOrderAddressAsync(Guid orderId,
			[FromBody] ChangeOrderAddressDTO command)
		{
			command.OrderId = orderId;
			await _orderingAppService.ChangeOrderAddress(command);
			return Ok();
		}

		#region QUERY

		[HttpGet("{orderId}")]
		public async Task<ActionResult> GetOrderAsync(string orderId)
		{
			var order = await _orderingAppService.GetOrderAsync(Guid.NewGuid());
			return Ok(order);
		}

		#endregion
	}
}