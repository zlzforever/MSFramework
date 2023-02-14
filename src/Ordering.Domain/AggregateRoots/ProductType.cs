using MicroserviceFramework.Domain;

namespace Ordering.Domain.AggregateRoots;

public class ProductType : Enumeration
{
    public static ProductType Apple = new ProductType(nameof(Apple), nameof(Apple));

    public static ProductType Banana =
        new ProductType(nameof(Banana), nameof(Banana));

    public ProductType(string id, string name) : base(id, name)
    {
    }
}
