using System;
using MSFramework.Application;

namespace Ordering.Application.Command
{
	public class CancelOrderCommand : ICommand
	{
		public Guid OrderId { get; private set; }

		public CancelOrderCommand(Guid orderId)
		{
			OrderId = orderId;
		}
	}
}