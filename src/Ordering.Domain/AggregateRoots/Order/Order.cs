using System.ComponentModel;
using MicroserviceFramework.Domain;
using MongoDB.Bson;
using Ordering.Domain.AggregateRoots.Events;

namespace Ordering.Domain.AggregateRoots.Order;

[Description("订单表")]
public class Order : DeletionAggregateRoot<string>, IOptimisticLock
{
    private readonly HashSet<string> _listJson;
    private readonly Dictionary<string, string> _dictJson;
    private readonly List<OrderExtra> _extras;

    // DDD Patterns comment
    // Using a private collection field, better for DDD Aggregate's encapsulation
    // so Items cannot be added from "outside the AggregateRoot" directly to the collection,
    // but only through the method OrderAggregateRoot.AddOrderItem() which includes behaviour.
    private readonly List<OrderItem> _items;

    /// <summary>
    /// Address is a Value Object pattern example persisted as EF Core 2.0 owned entity
    /// </summary>
    public Address Address { get; private set; }

    public ObjectId TestId { get; private set; }

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
    public string BuyerId { get; private set; }

    public string Description { get; private set; }

    public User Operator { get; private set; }

    /// <summary>
    /// 测试 List 的 JSON
    /// </summary>
    public IReadOnlyCollection<string> ListJson => _listJson;

    /// <summary>
    /// 测试对象列表的 JSON 存储
    /// </summary>
    public IReadOnlyCollection<OrderExtra> Extras => _extras;

    /// <summary>
    /// 用于测试字典 JSON 在 EF 中的序列化与反序列化
    /// KEY 的大小写差异
    /// </summary>
    public IReadOnlyDictionary<string, string> DictJson => _dictJson;

    public void SetList(IEnumerable<string> list)
    {
        foreach (var item in list)
        {
            _listJson.Add(item);
        }
    }

    public void SetOperator(User user)
    {
        Operator = user;
    }

    public void AddExtra(string key, string value)
    {
        _extras.Add(new OrderExtra(key, value));
    }

    public void AddKeyValue(string key, string value)
    {
        _dictJson.TryAdd(key, value);
    }

    private Order(string id) : base(id)
    {
        _items = [];
        _listJson = [];
        _dictJson = new Dictionary<string, string>();
        _extras = [];
        TestId = ObjectId.GenerateNewId();
        AddDomainEvent(new OrderCreatedDomainEvent { Id = id, CreationTime = DateTimeOffset.Now, Name = "test" });
    }

    public static Order Create(string buyerId,
        Address address,
        string description)
    {
        var order = new Order(ObjectId.GenerateNewId().ToString())
        {
            Address = address, BuyerId = buyerId, Description = description, Status = OrderStatus.Submitted
        };

        // Add the OrderStarterDomainEvent to the domain events collection
        // to be raised/dispatched when comitting changes into the Database [ After DbContext.SaveChanges() ]
        var orderStartedDomainEvent = new OrderStartedDomainEvent(order, buyerId);
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
            var product = new OrderProduct(productId, productName, pictureUrl);
            var orderItem = OrderItem.Create(this, product, unitPrice, units, discount);
            _items.Add(orderItem);
            return orderItem;
        }
    }

    public void SetAddress(Address newAddress)
    {
        Address = newAddress ?? throw new ArgumentException(nameof(newAddress));
    }

    public void SetAwaitingValidationStatus()
    {
        if (Equals(Status, OrderStatus.Submitted))
        {
            AddDomainEvent(new OrderStatusChangedToAwaitingValidationDomainEvent(Id, Items));
            Status = OrderStatus.AwaitingValidation;
        }
    }

    public void SetStockConfirmedStatus()
    {
        if (Equals(Status, OrderStatus.AwaitingValidation))
        {
            AddDomainEvent(new OrderStatusChangedToStockConfirmedDomainEvent(Id));

            Status = OrderStatus.StockConfirmed;
            Description = "All the items were confirmed with available stock.";
        }
    }

    public void SetPaidStatus()
    {
        if (Equals(Status, OrderStatus.StockConfirmed))
        {
            AddDomainEvent(new OrderStatusChangedToPaidDomainEvent(Id, Items));

            Status = OrderStatus.Paid;
            Description =
                "The payment was performed at a simulated \"American Bank checking bank account ending on XX35071\"";
        }
    }

    public void AddEvent()
    {
        AddDomainEvent(new EmptyEvent());
    }

    public void SetShippedStatus()
    {
        if (!Equals(Status, OrderStatus.Paid))
        {
            StatusChangeException(OrderStatus.Shipped);
        }

        Status = OrderStatus.Shipped;
        Description = "The order was shipped.";
        AddDomainEvent(new OrderShippedDomainEvent(this));
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

    public void SetCancelledStatusWhenStockIsRejected(IEnumerable<string> orderStockRejectedItems)
    {
        if (Equals(Status, OrderStatus.AwaitingValidation))
        {
            Status = OrderStatus.Cancelled;

            var itemsStockRejectedProductNames = Items
                .Where(c => orderStockRejectedItems.Contains(c.Product.ProductId))
                .Select(c => c.Product.Name);

            var itemsStockRejectedDescription = string.Join(", ", itemsStockRejectedProductNames);
            Description = $"The product items don't have stock: ({itemsStockRejectedDescription}).";
        }
    }

    private void StatusChangeException(OrderStatus orderStatusToChange)
    {
        throw new OrderingDomainException(
            $"Is not possible to change the order status from {Status} to {orderStatusToChange}.");
    }

    public string ConcurrencyStamp { get; set; }
}
