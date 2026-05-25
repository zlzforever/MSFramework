using MicroserviceFramework.Domain;

namespace Ordering.Domain.AggregateRoots.Order;

public record Product(string ProductId, string Name, string PictureUrl) : ValueObject;
