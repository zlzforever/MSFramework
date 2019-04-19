using System;
using System.Collections.Generic;
using System.Net;
using System.Threading.Tasks;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using MSFramework.Security;
using Ordering.API.Application.Dto.Order;
using Ordering.API.Application.Query;
using Ordering.Domain.AggregateRoot.Buyer;
using Ordering.Domain.AggregateRoot.Order;

namespace Ordering.API.Controllers
{
	[Route("api/v1/[controller]")]
	//[Authorize]
	[ApiController]
	public class OrderController : ControllerBase
	{
		private readonly IMediator _mediator;
		private readonly ILogger _logger;
		private readonly ICurrentUser _currentUser;

		public OrderController(IMediator mediator, ICurrentUser currentUser, ILogger<OrderController> logger)
		{
			_mediator = mediator;
			_logger = logger;
			_currentUser = currentUser;
		}

		#region QUERY

		[Route("{orderId:int}")]
		[HttpGet]
		[ProducesResponseType(typeof(Order), (int) HttpStatusCode.OK)]
		[ProducesResponseType((int) HttpStatusCode.NotFound)]
		public async Task<ActionResult> GetOrderAsync(Guid orderId)
		{
			try
			{
				var order = await _mediator.Send(new GetOrderByIdQuery(orderId));
				return Ok(order);
			}
			catch
			{
				return NotFound();
			}
		}
 

		#endregion
	}
}