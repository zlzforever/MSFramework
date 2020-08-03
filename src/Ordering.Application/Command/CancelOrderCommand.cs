using System;
using MSFramework.Application;
using MSFramework.Common;

namespace Ordering.Application.Command
{
	public class CancelOrderCommand : IRequest
	{
		public ObjectId OrderId { get; private set; }

		public CancelOrderCommand(ObjectId orderId)
		{
			OrderId = orderId;
		}
	}
}