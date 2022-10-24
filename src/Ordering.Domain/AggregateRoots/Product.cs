using MicroserviceFramework.Domain;
using MongoDB.Bson;

namespace Ordering.Domain.AggregateRoots;

public class ProjectCreatedEvent : DomainEvent
{
    public ObjectId Id { get; set; }
}

public class Product : AggregateRoot<ObjectId>, IOptimisticLock
{
    private Product(ObjectId id) : base(id)
    {
    }

    public static Product Create(string name, int price)
    {
        var product = new Product(ObjectId.GenerateNewId()) { Name = name, Price = price };
        product.AddDomainEvent(new ProjectCreatedEvent { Id = product.Id });

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
