using System;
using MongoDB.Bson;

namespace MicroserviceFramework.Domain;

/// <summary>
/// 含创建人信息的聚合根
/// </summary>
public abstract class CreationAggregateRoot : CreationAggregateRoot<ObjectId>
{
    protected CreationAggregateRoot(ObjectId id) : base(id)
    {
    }
}

/// <summary>
/// 含创建人信息的聚合根
/// </summary>
/// <typeparam name="TKey">主键</typeparam>
public abstract class CreationAggregateRoot<TKey> : CreationEntity<TKey>, IAggregateRoot<TKey>
    where TKey : IEquatable<TKey>
{
    protected CreationAggregateRoot(TKey id) : base(id)
    {
    }
}
