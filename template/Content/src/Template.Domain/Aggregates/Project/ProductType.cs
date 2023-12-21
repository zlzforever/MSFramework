using MicroserviceFramework.Domain;

namespace Template.Domain.Aggregates.Project;

public class ProductType(string id, string name) : Enumeration(id, name)
{
    /// <summary>
    /// 家居
    /// </summary>
    public static ProductType Home = new(nameof(Home), nameof(Home));

    /// <summary>
    /// 美妆
    /// </summary>
    public static ProductType Beauty = new(nameof(Beauty), nameof(Beauty));
}
