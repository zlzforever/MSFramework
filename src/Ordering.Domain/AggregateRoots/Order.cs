using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using MicroserviceFramework.Domain;
using Microsoft.EntityFrameworkCore.Infrastructure;
using MongoDB.Bson;
using Ordering.Domain.AggregateRoots.Events;

namespace Ordering.Domain.AggregateRoots;

[Description("订单表")]
public class Order : CreationAggregateRoot, IOptimisticLock
{
    private readonly HashSet<string> _rivalNetworks;
    private readonly Dictionary<string, string> _dict;
    private readonly ILazyLoader _lazyLoader;
    private readonly List<ExtraInfo> _extras;
    private User _creator2;

    // DDD Patterns comment
    // Using a private collection field, better for DDD Aggregate's encapsulation
    // so Items cannot be added from "outside the AggregateRoot" directly to the collection,
    // but only through the method OrderAggregateRoot.AddOrderItem() which includes behaviour.
    private List<OrderItem> _items;

    /// <summary>
    /// 用于测试字典 JSON 在 EF 中的序列化与反序列化
    /// KEY 的大小写差异
    /// </summary>
    public IReadOnlyDictionary<string, string> Dict => _dict;

    /// <summary>
    /// Address is a Value Object pattern example persisted as EF Core 2.0 owned entity
    /// </summary>
    public Address Address { get; private set; }

    /// <summary>
    /// 测试 List 的 JSON
    /// </summary>
    public IReadOnlyCollection<string> RivalNetworks => _rivalNetworks;

    /// <summary>
    /// 测试对象列表的 JSON 存储 
    /// </summary>
    public IReadOnlyCollection<ExtraInfo> Extras => _extras;

    public IReadOnlyCollection<OrderItem> Items => _lazyLoader.Load(this, ref _items);

    public OrderStatus Status { get; private set; }

    public string BuyerId { get; private set; }

    public string Description { get; private set; }

    public User Creator2 => _lazyLoader.Load(this, ref _creator2);

    public void SetRivalNetwork(IEnumerable<string> rivalNetworks)
    {
        foreach (var rivalNetwork in rivalNetworks)
        {
            _rivalNetworks.Add(rivalNetwork);
        }
    }

    public void SetCreator(User creator)
    {
        _creator2 = creator;
    }

    public void AddExtra(string name, string age)
    {
        _extras.Add(new ExtraInfo(name, age));
    }

    public void AddKeyValue(string key, string value)
    {
        _dict.TryAdd(key, value);
    }

    // ReSharper disable once UnusedMember.Local
    private Order(ILazyLoader lazyLoader) : this(ObjectId.Empty)
    {
        _lazyLoader = lazyLoader;
    }

    private Order(ObjectId id) : base(id)
    {
        _items = new List<OrderItem>();
        _rivalNetworks = new HashSet<string>();
        _dict = new Dictionary<string, string>();
        _extras = new List<ExtraInfo>();
    }

    private Order(
        string userId,
        Address address,
        string description
    ) : this(ObjectId.GenerateNewId())
    {
        Address = address;
        BuyerId = userId;
        Description = description;
        Status = OrderStatus.Submitted;

        // Add the OrderStarterDomainEvent to the domain events collection 
        // to be raised/dispatched when comitting changes into the Database [ After DbContext.SaveChanges() ]
        var orderStartedDomainEvent = new OrderStartedDomainEvent(this, userId);
        AddDomainEvent(orderStartedDomainEvent);
    }

    public static Order Create(string buyerId,
        Address address,
        string description)
    {
        return new Order(buyerId,
                address,
                description)
            ;
    }

    public OrderItem AddItem(Guid productId, string productName, decimal unitPrice, decimal discount,
        string pictureUrl, int units = 1)
    {
        var existingOrderForProduct = Items
            .SingleOrDefault(o => o.ProductId == productId);

        if (existingOrderForProduct != null)
        {
            //if previous line exist modify it with higher discount  and units..

            if (discount > existingOrderForProduct.Discount)
            {
                existingOrderForProduct.SetNewDiscount(discount);
            }

            existingOrderForProduct.AddUnits(units);
            return existingOrderForProduct;
        }
        else
        {
            //add validated new order item

            var orderItem = OrderItem.Create(productId, productName, unitPrice, discount, pictureUrl, units);
            _items.Add(orderItem);
            return orderItem;
        }
    }

    public void ChangeAddress(Address newAddress)
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

    public void SetCancelledStatusWhenStockIsRejected(IEnumerable<Guid> orderStockRejectedItems)
    {
        if (Equals(Status, OrderStatus.AwaitingValidation))
        {
            Status = OrderStatus.Cancelled;

            var itemsStockRejectedProductNames = Items
                .Where(c => orderStockRejectedItems.Contains(c.ProductId))
                .Select(c => c.ProductName);

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
