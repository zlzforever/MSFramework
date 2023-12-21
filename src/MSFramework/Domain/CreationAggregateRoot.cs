using System;
using MongoDB.Bson;

namespace MicroserviceFramework.Domain;

/// <summary>
/// 含创建人信息的聚合根
/// </summary>
public abstract class CreationAggregateRoot(ObjectId id) : CreationAggregateRoot<ObjectId>(id);

/// <summary>
/// 含创建人信息的聚合根
/// </summary>
/// <typeparam name="TKey">主键</typeparam>
public abstract class CreationAggregateRoot<TKey>(TKey id) : CreationEntity<TKey>(id), IAggregateRoot<TKey>
    where TKey : IEquatable<TKey>;
