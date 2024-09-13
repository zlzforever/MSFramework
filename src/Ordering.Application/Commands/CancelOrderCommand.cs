using MicroserviceFramework.Mediator;

namespace Ordering.Application.Commands;

public record CancelOrderCommand(string OrderId) : Request<string>
{
    public string OrderId { get; private set; } = OrderId;
}
