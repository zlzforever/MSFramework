using System;
using System.Collections.Generic;
using System.Net;
using System.Threading.Tasks;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using MSFramework.Security;
using Ordering.API.Application.Query;
using Ordering.Domain.Aggregates;

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

		public OrderController(IOrderService orderService, ICurrentUser currentUser, ILogger<OrderController> logger)
		{
			_orderService = orderService;
			_logger = logger;
			_currentUser = currentUser;
		}

		#region QUERY

		[Route("{orderId}")]
		[HttpGet]
		public async Task<ActionResult> GetOrderAsync(Guid orderId)
		{
			var order = await _orderService.GetOrderAsync(orderId);
			return Ok(order);
		}

		#endregion
	}
}