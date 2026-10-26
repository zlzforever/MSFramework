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

    /// <summary>添加或合并商品行，在修改聚合前验证数量与整行折扣。</summary>
    /// <param name="productId">服务端商品标识，不能为空白。</param>
    /// <param name="productName">服务端商品名称，不能为空白。</param>
    /// <param name="pictureUrl">商品图片，可为空。</param>
    /// <param name="unitPrice">服务端单价，不能为负。</param>
    /// <param name="units">追加数量，必须为正且合并后不能溢出。</param>
    /// <param name="discount">整行折扣，非负且不超过合并后的行金额。</param>
    /// <returns>新增或合并后的订单项。</returns>
    /// <exception cref="ArgumentException">商品标识或名称为空白。</exception>
    /// <exception cref="OrderingDomainException">价格、数量或折扣无效。</exception>
    public OrderItem AddItem(string productId, string productName, string pictureUrl, decimal unitPrice,
        int units = 1, decimal discount = 0)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(productId);
        ArgumentException.ThrowIfNullOrWhiteSpace(productName);
        if (unitPrice < 0 || units <= 0 || discount < 0)
        {
            throw new OrderingDomainException("Invalid price, units or discount");
        }

        var existingOrderForProduct = Items
            .SingleOrDefault(o => o.Product.ProductId == productId);

        if (existingOrderForProduct != null)
        {
            // 合并前完成全部校验，防止数量修改成功而后续折扣失败留下半更新的聚合。
            if (units > int.MaxValue - existingOrderForProduct.Units)
            {
                throw new OrderingDomainException("Invalid merged units or discount");
            }

            try
            {
                if (discount > existingOrderForProduct.Price * (existingOrderForProduct.Units + units))
                {
                    throw new OrderingDomainException("Invalid merged discount");
                }
            }
            catch (OverflowException exception)
            {
                throw new OrderingDomainException("Merged order item amount is too large", exception);
            }

            existingOrderForProduct.AddUnits(units);
            if (discount > existingOrderForProduct.Discount)
            {
                existingOrderForProduct.SetDiscount(discount);
            }

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

    /// <summary>
    /// 删除当前订单中的订单项。
    /// </summary>
    /// <param name="orderItemId">订单项标识</param>
    /// <returns>是否找到并移除订单项</returns>
    public bool DeleteItem(string orderItemId)
    {
        var orderItem = _items.SingleOrDefault(x => x.Id == orderItemId);
        return _items.Remove(orderItem);
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
