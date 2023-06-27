using System;
using MicroserviceFramework.Domain;
using Microsoft.EntityFrameworkCore.Infrastructure;
using MongoDB.Bson;

namespace Ordering.Domain.AggregateRoots;

public class OrderItem : EntityBase<ObjectId>
{
    private User _creator;

    private readonly ILazyLoader _lazyLoader;

    // DDD Patterns comment
    // Using private fields, allowed since EF Core 1.1, is a much better encapsulation
    // aligned with DDD Aggregates and Domain Entities (Instead of properties and property collections)
    public string ProductName { get; private set; }
    public string PictureUrl { get; private set; }
    public decimal UnitPrice { get; private set; }
    public decimal Discount { get; private set; }
    public int Units { get; private set; }
    public Guid ProductId { get; private set; }
    public User Creator => _lazyLoader.Load(this, ref _creator);

    private OrderItem(ILazyLoader lazyLoader) : this(ObjectId.Empty)
    {
        _lazyLoader = lazyLoader;
    }

    private OrderItem(ObjectId id) : base(id)
    {
    }

    public static OrderItem Create(Guid productId, string productName, decimal unitPrice,
        decimal discount,
        string pictureUrl,
        int units = 1)
    {
        if (units <= 0)
        {
            throw new OrderingDomainException("Invalid number of units");
        }

        if (unitPrice * units < discount)
        {
            throw new OrderingDomainException("The total of order item is lower than applied discount");
        }

        var item = new OrderItem(ObjectId.GenerateNewId())
        {
            ProductId = productId,
            ProductName = productName,
            UnitPrice = unitPrice,
            Discount = discount,
            Units = units,
            PictureUrl = pictureUrl
        };
        return item;
    }


    public void SetNewDiscount(decimal discount)
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

    public void SetCreator(User user)
    {
        _creator = user;
    }
}
