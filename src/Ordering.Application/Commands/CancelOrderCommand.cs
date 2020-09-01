using MicroserviceFramework.Application;
using MicroserviceFramework.Application.CQRS.Command;
using MicroserviceFramework.Shared;

namespace Ordering.Application.Commands
{
	public class CancelOrderCommand : ICommand<ObjectId>
	{
		public ObjectId OrderId { get; private set; }

		public CancelOrderCommand(ObjectId orderId)
		{
			OrderId = orderId;
		}
	}
}