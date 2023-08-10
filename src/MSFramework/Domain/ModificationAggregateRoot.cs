using System;
using MongoDB.Bson;

namespace MicroserviceFramework.Domain;

/// <summary>
/// 含最后修改信息的聚合根
/// </summary>
public abstract class ModificationAggregateRoot : ModificationAggregateRoot<ObjectId>
{
    protected ModificationAggregateRoot(ObjectId id) : base(id)
    {
    }
}

/// <summary>
/// 含最后修改信息的聚合根
/// </summary>
public abstract class ModificationAggregateRoot<TKey> : ModificationEntity<TKey>, IAggregateRoot<TKey>
    where TKey : IEquatable<TKey>
{
    protected ModificationAggregateRoot(TKey id) : base(id)
    {
    }
}
