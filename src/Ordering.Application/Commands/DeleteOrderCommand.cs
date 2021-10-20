using MicroserviceFramework.Mediator;
using MongoDB.Bson;

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