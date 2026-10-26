using MicroserviceFramework.Domain;
using MongoDB.Bson;

namespace Ordering.Domain.AggregateRoots.Order;

/// <summary>
/// 订单内的商品价格快照与数量，供订单聚合维护金额不变量；不依赖基础设施。
/// </summary>
public class OrderItem : DeletionEntity<string>
{
    /// <summary>所属订单，创建后不可为空。</summary>
    public Order Order { get; private set; }

    /// <summary>
    /// 销售产品
    /// </summary>
    public Product Product { get; private set; }

    /// <summary>
    /// 单价
    /// </summary>
    public decimal Price { get; private set; }

    /// <summary>
    /// 数量
    /// </summary>
    public int Units { get; private set; }

    /// <summary>
    /// 折扣
    /// </summary>
    public decimal Discount { get; private set; }

    private OrderItem(string id) : base(id)
    {
    }

    /// <summary>创建订单项，价格和折扣由服务端定价，折扣为整行金额。</summary>
    /// <param name="order">所属订单，不允许为空。</param>
    /// <param name="product">销售商品快照，不允许为空。</param>
    /// <param name="unitPrice">单价，与折扣使用相同货币单位，不能为负。</param>
    /// <param name="units">购买数量，必须大于零。</param>
    /// <param name="discount">整行折扣，必须在零至单价乘数量之间。</param>
    /// <returns>具有离线标识的新订单项。</returns>
    /// <exception cref="ArgumentNullException">订单或商品为空。</exception>
    /// <exception cref="OrderingDomainException">价格、数量或折扣违反金额约束。</exception>
    public static OrderItem Create(Order order, Product product, decimal unitPrice,
        int units, decimal discount)
    {
        ArgumentNullException.ThrowIfNull(order);
        ArgumentNullException.ThrowIfNull(product);
        if (units <= 0 || unitPrice < 0)
        {
            throw new OrderingDomainException("Invalid number of units");
        }

        ValidateAmount(unitPrice, units, discount);

        var item = new OrderItem(ObjectId.GenerateNewId().ToString())
        {
            Order = order,
            Product = product,
            Price = unitPrice,
            Discount = discount,
            Units = units
        };
        return item;
    }

    /// <summary>修改整行折扣，不允许折扣超过商品总额。</summary>
    /// <param name="discount">与单价同单位，必须非负且不超过整行金额。</param>
    /// <exception cref="OrderingDomainException">折扣为负或超过整行金额。</exception>
    public void SetDiscount(decimal discount)
    {
        ValidateAmount(Price, Units, discount);
        Discount = discount;
    }

    /// <summary>追加购买数量，拒绝零、负数及整数溢出。</summary>
    /// <param name="units">追加数量，必须为正且相加后不超过 Int32.MaxValue。</param>
    /// <exception cref="OrderingDomainException">追加数量无效或导致溢出。</exception>
    public void AddUnits(int units)
    {
        if (units <= 0 || units > int.MaxValue - Units)
        {
            throw new OrderingDomainException("Invalid units");
        }

        ValidateAmount(Price, Units + units, Discount);
        Units += units;
    }

    /// <summary>确保整行金额能够由 decimal 表示且折扣不超过总额。</summary>
    private static void ValidateAmount(decimal price, int units, decimal discount)
    {
        try
        {
            if (discount < 0 || discount > price * units)
            {
                throw new OrderingDomainException("Discount is not valid");
            }
        }
        catch (OverflowException exception)
        {
            throw new OrderingDomainException("Order item amount is too large", exception);
        }
    }
}
