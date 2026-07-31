using System;
using MicroserviceFramework.Domain;

namespace Ordering.Domain.AggregateRoots.CompositeKey;

/// <summary>
/// 多列复合主键聚合根示例（方案 B：实体自身多个属性直接作主键，无 Id 包装）。
/// <para>
/// 实现非泛型 <see cref="IAggregateRoot"/>，以 <c>OrderId</c> + <c>ProductId</c>
/// 两个标量属性直接作为主键（EF 顶层标量多列 <c>HasKey</c>，EF Core 10 原生支持），
/// 配合无键仓储 <see cref="IRepository{TAggregateRoot}"/> 使用表达式谓词查询。
/// </para>
/// </summary>
public class MultiColumnOrderItem : EntityBase, IAggregateRoot
{
    /// <summary>
    /// 供 EF Core 物化使用的受保护无参构造
    /// </summary>
    protected MultiColumnOrderItem()
    {
    }

    private MultiColumnOrderItem(string orderId, string productId, string name, int quantity)
    {
        OrderId = orderId;
        ProductId = productId;
        Name = name;
        Quantity = quantity;
    }

    /// <summary>
    /// 订单标识（复合主键成员 1）
    /// </summary>
    public string OrderId { get; private set; }

    /// <summary>
    /// 产品标识（复合主键成员 2）
    /// </summary>
    public string ProductId { get; private set; }

    /// <summary>
    /// 产品名称
    /// </summary>
    public string Name { get; private set; }

    /// <summary>
    /// 数量
    /// </summary>
    public int Quantity { get; private set; }

    /// <summary>
    /// 创建多列复合主键聚合根
    /// </summary>
    /// <param name="orderId">订单标识（复合主键成员 1）</param>
    /// <param name="productId">产品标识（复合主键成员 2）</param>
    /// <param name="name">产品名称</param>
    /// <param name="quantity">数量</param>
    /// <returns>新创建的聚合根</returns>
    /// <exception cref="ArgumentOutOfRangeException">数量小于等于 0 时抛出</exception>
    public static MultiColumnOrderItem Create(string orderId, string productId, string name, int quantity)
    {
        if (quantity <= 0)
        {
            throw new ArgumentOutOfRangeException(nameof(quantity), "数量必须大于 0");
        }

        return new MultiColumnOrderItem(orderId, productId, name, quantity);
    }
}
