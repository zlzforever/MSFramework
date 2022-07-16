using MicroserviceFramework.Mediator;
using MongoDB.Bson;

namespace Ordering.Application.Commands
{
    public class CancelOrderCommand : IRequest<ObjectId>
    {
        public ObjectId OrderId { get; private set; }

        public CancelOrderCommand(ObjectId orderId)
        {
            OrderId = orderId;
        }
    }
}