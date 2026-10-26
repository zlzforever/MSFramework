using MicroserviceFramework.Domain;

namespace Template.Domain.Aggregates;

/// <summary>不可变商品类别，共享静态实例供 EF 与 JSON 转换器解析，不依赖基础设施。</summary>
public sealed class ProductType : Enumeration
{
    /// <summary>家居商品。</summary>
    public static readonly ProductType Home = new(nameof(Home));

    /// <summary>美妆商品。</summary>
    public static readonly ProductType Beauty = new(nameof(Beauty));

    private ProductType(string id) : base(id, id) { }
}
