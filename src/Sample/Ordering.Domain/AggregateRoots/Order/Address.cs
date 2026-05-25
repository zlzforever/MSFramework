using MicroserviceFramework.Domain;

namespace Ordering.Domain.AggregateRoots.Order;

public record Address
    : ValueObject
{
    public string Street { get; init; }
    public string City { get; init; }
    public string State { get; init; }
    public string Country { get; init; }
    public string ZipCode { get; init; }
}
