using MicroserviceFramework.Domain;

namespace Ordering.Domain.AggregateRoots.Order;

public record OrderProduct(string ProductId, string Name, string PictureUrl) : ValueObject;
