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
