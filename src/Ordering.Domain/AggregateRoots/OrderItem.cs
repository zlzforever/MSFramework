using MicroserviceFramework.Domain;
using MongoDB.Bson;

namespace Ordering.Domain.AggregateRoots;

public class OrderItem : EntityBase<string>
{
    public Order Order { get; private set; }

    /// <summary>
    /// 销售产品
    /// </summary>
    public OrderProduct Product { get; private set; }

    /// <summary>
    /// 单价
    /// </summary>
    public decimal UnitPrice { get; private set; }

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

    public static OrderItem Create(Order order, OrderProduct product, decimal unitPrice,
        int units, decimal discount)
    {
        if (units <= 0)
        {
            throw new OrderingDomainException("Invalid number of units");
        }

        if (unitPrice * units < discount)
        {
            throw new OrderingDomainException("The total of order item is lower than applied discount");
        }

        var item = new OrderItem(ObjectId.GenerateNewId().ToString())
        {
            Order = order,
            Product = product,
            UnitPrice = unitPrice,
            Discount = discount,
            Units = units
        };
        return item;
    }

    public void SetDiscount(decimal discount)
    {
        if (discount < 0)
        {
            throw new OrderingDomainException("Discount is not valid");
        }

        Discount = discount;
    }

    public void AddUnits(int units)
    {
        if (units < 0)
        {
            throw new OrderingDomainException("Invalid units");
        }

        Units += units;
    }
}
