using System;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace Ordering.Application.Command
{
	public class DeleteOrderCommand : IRequest<IActionResult>
	{
		public Guid OrderId { get; private set; }

		public DeleteOrderCommand(Guid orderId)
		{
			OrderId = orderId;
		}
	}
}