using System;
using MicroserviceFramework.Domain;

namespace Ordering.Domain.AggregateRoots.CompositeKey;

/// <summary>
/// 复合主键聚合根示例（方案 A：值对象键）。
/// <para>
/// 主键类型为 <see cref="OrderItemKey"/> 值对象（不可变 record），
/// 通过 <c>ConfigureCompositeKey</c>（ValueConverter 单列映射）持久化，
/// 现有 <see cref="IRepository{TEntity,TKey}"/> 体系无需任何改动即可直接使用。
/// </para>
/// </summary>
public class CompositeOrderItem : DeletionAggregateRoot<OrderItemKey>
{
    /// <summary>
    /// 产品名称
    /// </summary>
    public string Name { get; private set; }

    /// <summary>
    /// 数量
    /// </summary>
    public int Quantity { get; private set; }

    /// <summary>
    /// 供 EF Core 物化使用的受保护无参构造
    /// </summary>
    protected CompositeOrderItem() : base(default!)
    {
    }

    private CompositeOrderItem(OrderItemKey id, string name, int quantity) : base(id)
    {
        Name = name;
        Quantity = quantity;
    }

    /// <summary>
    /// 创建复合主键订单项聚合根
    /// </summary>
    /// <param name="orderId">订单标识（复合主键成员 1）</param>
    /// <param name="productId">产品标识（复合主键成员 2）</param>
    /// <param name="name">产品名称</param>
    /// <param name="quantity">数量</param>
    /// <returns>新创建的聚合根</returns>
    /// <exception cref="ArgumentOutOfRangeException">数量小于等于 0 时抛出</exception>
    public static CompositeOrderItem Create(string orderId, string productId, string name, int quantity)
    {
        if (quantity <= 0)
        {
            throw new ArgumentOutOfRangeException(nameof(quantity), "数量必须大于 0");
        }

        return new CompositeOrderItem(new OrderItemKey(orderId, productId), name, quantity);
    }
}
