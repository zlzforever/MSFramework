using MicroserviceFramework;
using MicroserviceFramework.Domain;
using MongoDB.Bson;

namespace Template.Domain.Aggregates;

/// <summary>商品聚合，负责名称、分单位价格和软删除，依赖框架领域基类与离线 ObjectId。</summary>
public class Product : DeletionAggregateRoot
{
    private Product(ObjectId id) : base(id) { }

    /// <summary>商品名称，去除首尾空白后为 1 至 100 字符。</summary>
    public string Name { get; private set; } = string.Empty;

    /// <summary>商品价格，单位为分，必须非负。</summary>
    public int Price { get; private set; }

    /// <summary>固定枚举商品类别，不能为空。</summary>
    public ProductType ProductType { get; private set; } = ProductType.Home;

    /// <summary>创建商品并记录待同事务派发的领域事件。</summary>
    /// <param name="name">商品名称，去除首尾空白后长度为 1 至 100。</param>
    /// <param name="price">价格，单位为分，不能为负。</param>
    /// <param name="productType">商品类别，不能为空。</param>
    /// <returns>具有离线标识及创建事件的新聚合。</returns>
    /// <exception cref="ArgumentNullException">商品类别为空。</exception>
    /// <exception cref="MicroserviceFrameworkFriendlyException">名称或价格不满足约束。</exception>
    public static Product Create(string name, int price, ProductType productType)
    {
        ArgumentNullException.ThrowIfNull(productType);
        name = name?.Trim() ?? string.Empty;
        if (name.Length is < 1 or > 100 || price < 0)
        {
            throw new MicroserviceFrameworkFriendlyException("商品名称或价格不合法。");
        }

        var product = new Product(ObjectId.GenerateNewId())
        {
            Name = name, Price = price, ProductType = productType
        };
        product.AddDomainEvent(new ProductCreatedEvent(product.Id));
        return product;
    }
}
