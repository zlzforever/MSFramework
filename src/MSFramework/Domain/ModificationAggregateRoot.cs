using System;
using MongoDB.Bson;

namespace MicroserviceFramework.Domain;

/// <summary>
/// 含修改审计信息的聚合根
/// </summary>
public abstract class ModificationAggregateRoot(ObjectId id) : ModificationAggregateRoot<ObjectId>(id);

/// <summary>
/// 含修改审计信息的聚合根
/// </summary>
public abstract class ModificationAggregateRoot<TKey>(TKey id) : ModificationEntity<TKey>(id), IAggregateRoot<TKey>
    where TKey : IEquatable<TKey>;
