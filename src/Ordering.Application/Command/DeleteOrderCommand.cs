using System;
using MSFramework.Application;
using MSFramework.Common;

namespace Ordering.Application.Command
{
	public class DeleteOrderCommand : IRequest
	{
		public ObjectId OrderId { get; private set; }

		public DeleteOrderCommand(ObjectId orderId)
		{
			OrderId = orderId;
		}
	}
}