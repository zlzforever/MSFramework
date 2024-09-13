using System;
using MongoDB.Bson;

namespace MicroserviceFramework.Domain;

/// <summary>
/// 含删除审计信息的聚合根
/// </summary>
public abstract class DeletionAggregateRoot(ObjectId id) : DeletionAggregateRoot<ObjectId>(id);

/// <summary>
/// 含删除审计信息的聚合根
/// </summary>
public abstract class DeletionAggregateRoot<TKey>(TKey id) : DeletionEntity<TKey>(id), IAggregateRoot<TKey>
    where TKey : IEquatable<TKey>;
