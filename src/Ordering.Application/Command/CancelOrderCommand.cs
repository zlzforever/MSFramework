using System;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace Ordering.Application.Command
{
	public class CancelOrderCommand : IRequest<IActionResult>
	{
		public Guid OrderId { get; private set; }

		public CancelOrderCommand(Guid orderId)
		{
			OrderId = orderId;
		}
	}
}