using System;
using MicroserviceFramework.Domain;
using MongoDB.Bson;
using Ordering.Domain.AggregateRoots.Events;

namespace Ordering.Domain.AggregateRoots;

public class Product : CreationAggregateRoot, IOptimisticLock
{
    private Product(ObjectId id) : base(id)
    {
    }

    public static Product Create(string name, int price)
    {
        var product = new Product(ObjectId.GenerateNewId()) { Name = name, Price = price };
        product.AddDomainEvent(new ProjectCreatedEvent
        {
            Id = product.Id,
            Name = name,
            CreationTime = DateTimeOffset.Now
        });

        return product;
    }

    public static Product CreateWithoutEvent(string name, int price)
    {
        var product = new Product(ObjectId.GenerateNewId()) { Name = name, Price = price };


        return product;
    }

    public string Name { get; private set; }

    public int Price { get; private set; }

    public override string ToString()
    {
        return $"[ENTITY {GetType().Name}] Id = {Id}, Name = {Name}, Price = {Price}";
    }

    public string ConcurrencyStamp { get; set; }

    public void SetName(string name)
    {
        Name = name;
    }
}
