using MicroserviceFramework.Mediator;
using MongoDB.Bson;

namespace Ordering.Application.Commands;

public record CancelOrderCommand : Request<ObjectId>
{
    public ObjectId OrderId { get; private set; }

    public CancelOrderCommand(ObjectId orderId)
    {
        OrderId = orderId;
    }
}
