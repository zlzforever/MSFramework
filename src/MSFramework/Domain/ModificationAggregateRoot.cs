using System;
using MongoDB.Bson;

namespace MicroserviceFramework.Domain;

public abstract class ModificationAggregateRoot : ModificationAggregateRoot<ObjectId>
{
    protected ModificationAggregateRoot(ObjectId id) : base(id)
    {
    }
}

public abstract class ModificationAggregateRoot<TKey> : ModificationEntity<TKey>, IAggregateRoot<TKey>
    where TKey : IEquatable<TKey>
{
    protected ModificationAggregateRoot(TKey id) : base(id)
    {
    }
}
