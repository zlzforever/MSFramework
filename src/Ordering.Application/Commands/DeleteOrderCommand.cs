using MicroserviceFramework.Application.CQRS;
using MongoDB.Bson;

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