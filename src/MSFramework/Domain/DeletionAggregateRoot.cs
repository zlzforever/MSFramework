using System;
using MongoDB.Bson;

namespace MicroserviceFramework.Domain;

/// <summary>
/// 含删除人信息的聚合根
/// </summary>
public abstract class DeletionAggregateRoot : DeletionAggregateRoot<ObjectId>
{
    protected DeletionAggregateRoot(ObjectId id) : base(id)
    {
    }
}

/// <summary>
/// 含删除人信息的聚合根
/// </summary>
public abstract class DeletionAggregateRoot<TKey> : DeletionEntity<TKey>, IAggregateRoot<TKey>
    where TKey : IEquatable<TKey>
{
    protected DeletionAggregateRoot(TKey id) : base(id)
    {
    }
}
