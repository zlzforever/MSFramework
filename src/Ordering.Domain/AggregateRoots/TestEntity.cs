using MicroserviceFramework.Domain;
using MongoDB.Bson;

namespace Ordering.Domain.AggregateRoots;

public class TestEntity : CreationAggregateRoot
{
    public string Name { get; private set; }

    private TestEntity(ObjectId id) : base(id)
    {
    }
}

public class TestEntity2 : CreationAggregateRoot<long>
{
    public string Name { get; private set; }

    private TestEntity2(long id) : base(id)
    {
    }
}

