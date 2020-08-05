using MSFramework.Application;
using MSFramework.Shared;

namespace Ordering.Application.Commands
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