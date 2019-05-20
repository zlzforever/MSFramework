using System;
using MediatR;

namespace Ordering.Application.Command
{
	public class DeleteOrderCommand : IRequest<bool>
	{
		public Guid OrderId { get; private set; }

		public DeleteOrderCommand(Guid orderId)
		{
			OrderId = orderId;
		}
	}
}