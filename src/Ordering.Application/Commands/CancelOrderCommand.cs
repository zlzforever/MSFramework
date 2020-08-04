using MSFramework.Application;
using MSFramework.Common;

namespace Ordering.Application.Commands
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