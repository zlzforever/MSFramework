using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using MSFramework.AspNetCore;
using MSFramework.Domain;
using Ordering.API.Application.DTO;
using Ordering.API.Application.Services;

namespace Ordering.API.Controllers
{
	[Route("api/v1/[controller]")]
	[ApiController]
	public class OrderController : MSFrameworkControllerBase
	{
		private readonly IOrderingAppService _orderingAppService;

		public OrderController(
			IOrderingAppService orderingAppService,
			IMSFrameworkSession session, ILogger<OrderController> logger) : base(session, logger)
		{
			_orderingAppService = orderingAppService;			 
		}

		#region  Command

		/// <summary>
		/// FOR TEST Method
		/// </summary>
		/// <returns></returns>
		[HttpPost]
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
			return Ok(true);
		}

		[HttpDelete("{orderId}")]
		public async Task<IActionResult> DeleteOrderAsync(Guid orderId)
		{
			await _orderingAppService.DeleteOrder(new DeleteOrderDto
			{
				OrderId = orderId
			});
			return Ok(true);
		}

		[HttpPut("{orderId}/address")]
		public async Task<IActionResult> ChangeOrderAddressAsync(Guid orderId,
			[FromBody] ChangeOrderAddressDTO dto)
		{
			dto.OrderId = orderId;
			await _orderingAppService.ChangeOrderAddress(dto);
			return Ok(true);
		}

		#endregion

		#region QUERY

		[HttpGet("{orderId}")]
		public async Task<ActionResult> GetOrderAsync(Guid orderId)
		{
			var order = await _orderingAppService.GetOrderAsync(orderId);
			return Ok(order);
		}

		[HttpGet()]
		public async Task<ActionResult> GetOrdersAsync()
		{
			var order = await _orderingAppService.GetAllOrdersAsync();
			return Ok(order);
		}
		
		#endregion
	}
}