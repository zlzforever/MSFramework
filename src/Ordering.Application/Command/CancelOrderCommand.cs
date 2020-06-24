using System;
using MediatR;

namespace Ordering.Application.Command
{
	public class CancelOrderCommand : IRequest
	{
		public Guid OrderId { get; private set; }

		public CancelOrderCommand(Guid orderId)
		{
			OrderId = orderId;
		}
	}
}