using MicroserviceFramework.Application.CQRS;
using MongoDB.Bson;

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