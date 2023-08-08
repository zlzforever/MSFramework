using MicroserviceFramework.Domain;
using MongoDB.Bson;

namespace Ordering.Domain.AggregateRoots;

public record OrderProduct : ValueObject
{
    /// <summary>
    /// 产品标识
    /// </summary>
    public string ProductId { get; private set; }

    // DDD Patterns comment
    // Using private fields, allowed since EF Core 1.1, is a much better encapsulation
    // aligned with DDD Aggregates and Domain Entities (Instead of properties and property collections)
    public string Name { get; private set; }

    /// <summary>
    /// 图片链接
    /// </summary>
    public string PictureUrl { get; private set; }

    public static OrderProduct Create(string productId, string name, string pictureUrl)
    {
        return new OrderProduct { ProductId = productId, Name = name, PictureUrl = pictureUrl };
    }
}
