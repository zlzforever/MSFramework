using System.ComponentModel;
using MicroserviceFramework.Domain;
using MongoDB.Bson;
using Ordering.Domain.AggregateRoots.Events;

namespace Ordering.Domain.AggregateRoots.Order;

[Description("订单表")]
public class Order : DeletionAggregateRoot<string>, IOptimisticLock
{
    private readonly HashSet<string> _list;
    private readonly Dictionary<string, string> _dict;
    private readonly List<ExtraInfo> _extra;

    // DDD Patterns comment
    // Using a private collection field, better for DDD Aggregate's encapsulation
    // so Items cannot be added from "outside the AggregateRoot" directly to the collection,
    // but only through the method OrderAggregateRoot.AddOrderItem() which includes behaviour.
    private readonly List<OrderItem> _items;

    /// <summary>
    /// Address is a Value Object pattern example persisted as EF Core 2.0 owned entity
    /// </summary>
    public Address Address { get; private set; }

    /// <summary>
    /// 订单项
    /// </summary>
    public IReadOnlyCollection<OrderItem> Items => _items;

    /// <summary>
    /// 订单状态
    /// </summary>
    public OrderStatus Status { get; private set; }

    /// <summary>
    /// 购买人员
    /// </summary>
    public UserInfo Buyer { get; private set; }

    /// <summary>
    /// 描述信息
    /// </summary>
    public string Description { get; private set; }

    /// <summary>
    /// 测试自动设置长度
    /// </summary>
    public ObjectId TestId { get; private set; }

    /// <summary>
    /// 测试 List 的 JSON
    /// </summary>
    public IReadOnlyCollection<string> List => _list;

    /// <summary>
    /// 测试对象列表的 JSON 存储
    /// </summary>
    public IReadOnlyCollection<ExtraInfo> Extra => _extra;

    /// <summary>
    /// 用于测试字典 JSON 在 EF 中的序列化与反序列化
    /// KEY 的大小写差异
    /// </summary>
    public IReadOnlyDictionary<string, string> Dict => _dict;

    public void SetList(IEnumerable<string> list)
    {
        foreach (var item in list)
        {
            _list.Add(item);
        }
    }

    public void AddExtra(string key, string value)
    {
        _extra.Add(new ExtraInfo(key, value));
    }

    public void AddKeyValuePair(string key, string value)
    {
        _dict.TryAdd(key, value);
    }

    private Order(string id) : base(id)
    {
        _items = [];
        _list = [];
        _dict = new Dictionary<string, string>();
        _extra = [];
        TestId = ObjectId.GenerateNewId();
    }

    public static Order Create(UserInfo buyer,
        Address address,
        string description)
    {
        var order = new Order(ObjectId.GenerateNewId().ToString())
        {
            Address = address, Buyer = buyer, Description = description, Status = OrderStatus.Submitted
        };

        // Add the OrderStarterDomainEvent to the domain events collection
        // to be raised/dispatched when committing changes into the Database [ After DbContext.SaveChanges() ]
        var orderStartedDomainEvent = new OrderStartedDomainEvent(order, buyer.Id);
        order.AddDomainEvent(orderStartedDomainEvent);
        return order;
    }

    public OrderItem AddItem(string productId, string productName, string pictureUrl, decimal unitPrice,
        int units = 1, decimal discount = 0)
    {
        var existingOrderForProduct = Items
            .SingleOrDefault(o => o.Product.ProductId == productId);

        if (existingOrderForProduct != null)
        {
            //if previous line exist modify it with higher discount  and units..

            if (discount > existingOrderForProduct.Discount)
            {
                existingOrderForProduct.SetDiscount(discount);
            }

            existingOrderForProduct.AddUnits(units);
            return existingOrderForProduct;
        }
        else
        {
            //add validated new order item
            var product = new Product(productId, productName, pictureUrl);
            var orderItem = OrderItem.Create(this, product, unitPrice, units, discount);
            _items.Add(orderItem);
            return orderItem;
        }
    }

    public void SetAddress(Address newAddress)
    {
        Address = newAddress ?? throw new ArgumentException(nameof(newAddress));
    }

    public void AddEvent()
    {
        AddDomainEvent(new EmptyEvent());
    }

    public void SetCancelledStatus()
    {
        if (Equals(Status, OrderStatus.Paid) ||
            Equals(Status, OrderStatus.Shipped))
        {
            StatusChangeException(OrderStatus.Cancelled);
        }

        Status = OrderStatus.Cancelled;
        Description = $"The order was cancelled.";
        AddDomainEvent(new OrderCancelledDomainEvent(this));
    }

    private void StatusChangeException(OrderStatus orderStatusToChange)
    {
        throw new OrderingDomainException(
            $"Is not possible to change the order status from {Status} to {orderStatusToChange}.");
    }

    public string ConcurrencyStamp { get; set; }
}
