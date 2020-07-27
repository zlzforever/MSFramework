using System;
using MSFramework.Application;

namespace Ordering.Application.Command
{
	public class DeleteOrderCommand : IRequest
	{
		public Guid OrderId { get; private set; }

		public DeleteOrderCommand(Guid orderId)
		{
			OrderId = orderId;
		}
	}
}