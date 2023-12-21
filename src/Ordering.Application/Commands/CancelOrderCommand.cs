using MicroserviceFramework.Mediator;
using MongoDB.Bson;

namespace Ordering.Application.Commands;

public record CancelOrderCommand(ObjectId OrderId) : Request<ObjectId>
{
    public ObjectId OrderId { get; private set; } = OrderId;
}
