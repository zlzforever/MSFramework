using MicroserviceFramework.Mediator;
using Ordering.Domain.AggregateRoots;

namespace Ordering.Application.Commands;

public record ChangeOrderAddressCommand : Request
{
    public Address NewAddress { get; set; }

    public string OrderId { get; set; }
}
