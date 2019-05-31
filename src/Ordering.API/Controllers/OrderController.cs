using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using MSFramework.AspNetCore;
using MSFramework.Domain;
using MSFramework.EventBus;
using Ordering.Application.Command;
using Ordering.Application.Event;
using Ordering.Application.Query;


namespace Ordering.API.Controllers
{
	[Route("api/v1.0/[controller]")]
	[ApiController]
	public class OrderController : MSFrameworkControllerBase
	{
		private readonly IOrderingQuery _orderingQuery;
		private readonly IEventBus _eventBus;
		private readonly IMediator _mediator;

		public OrderController(IMediator mediator, IEventBus eventBus,
			IOrderingQuery orderingQuery,
			IMSFrameworkSession session, ILogger<OrderController> logger) : base(session, logger)
		{
			_orderingQuery = orderingQuery;
			_eventBus = eventBus;
			_mediator = mediator;
		}
		[HttpGet("rabbitmq")]
		public async Task<IActionResult> Test()
		{
			await _eventBus.PublishAsync(new UserCheckoutAcceptedEvent(new List<UserCheckoutAcceptedEvent.OrderItemDTO>() { }
			, "aaa", "bbb", "ccc", "ddd", "eee", "fff", "xxx"));
			return Ok();
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
			var items = new List<CreateOrderCommand.OrderItemDTO>();
			var count = random.Next(2, 5);
			for (int i = 0; i < count; ++i)
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

			return await _mediator.Send(new CreateOrderCommand(items, "HELLO",
				"上海", "张扬路500号", "上海", "中国", "200000", "what?"));
		}

		[HttpDelete("{orderId}")]
		public async Task<IActionResult> DeleteOrderAsync(Guid orderId)
		{
			return await _mediator.Send(new DeleteOrderCommand(orderId));
		}

		[HttpPut("{orderId}/address")]
		public async Task<IActionResult> ChangeOrderAddressAsync(Guid orderId,
			[FromBody] ChangeOrderAddressCommand command)
		{
			command.OrderId = orderId;
			return await _mediator.Send(command);
		}

		#endregion

		#region QUERY

		[HttpGet("{orderId}")]
		public async Task<ActionResult> GetOrderAsync(Guid orderId)
		{
			var order = await _orderingQuery.GetOrderAsync(orderId);
			return Ok(order);
		}

		[HttpGet()]
		public async Task<ActionResult> GetOrdersAsync()
		{
			var order = await _orderingQuery.GetAllOrdersAsync();
			return Ok(order);
		}

		#endregion
	}
}