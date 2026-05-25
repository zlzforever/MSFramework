using MicroserviceFramework.Domain;

namespace Ordering.Domain.AggregateRoots;

public class ProductType(string id, string name) : Enumeration(id, name)
{
    public static ProductType Apple = new ProductType(nameof(Apple), nameof(Apple));

    public static ProductType Banana =
        new ProductType(nameof(Banana), nameof(Banana));
}
