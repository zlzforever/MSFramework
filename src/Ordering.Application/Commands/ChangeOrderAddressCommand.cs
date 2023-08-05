using MicroserviceFramework.Mediator;
using MongoDB.Bson;
using Ordering.Domain.AggregateRoots;

namespace Ordering.Application.Commands;

public record ChangeOrderAddressCommand : Request
{
    public Address NewAddress { get; set; }

    public ObjectId OrderId { get; set; }
}
