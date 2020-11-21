using MicroserviceFramework.Application.CQRS;
using MicroserviceFramework.Shared;

namespace Ordering.Application.Commands
{
	public class DeleteOrderCommand : ICommand
	{
		public ObjectId OrderId { get; private set; }

		public DeleteOrderCommand(ObjectId orderId)
		{
			OrderId = orderId;
		}
	}
}