using MicroserviceFramework.Domain;

namespace Ordering.Domain.AggregateRoots.Order;

public record Address(string Street, string City, string State, string Country, string ZipCode)
    : ValueObject;
