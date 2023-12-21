using MicroserviceFramework.Domain;

namespace Ordering.Domain.AggregateRoots;

public record OrderProduct(string ProductId, string Name, string PictureUrl) : ValueObject;
