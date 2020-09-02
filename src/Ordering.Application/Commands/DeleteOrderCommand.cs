using MicroserviceFramework.Application.CQRS.Command;
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