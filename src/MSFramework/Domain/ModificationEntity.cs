using System;
using MongoDB.Bson;

namespace MicroserviceFramework.Domain;

/// <summary>
/// 含最后修改人信息的实体
/// </summary>
public abstract class ModificationEntity(ObjectId id) : ModificationEntity<ObjectId>(id);

/// <summary>
/// 含最后修改人信息的实体
/// </summary>
public abstract class ModificationEntity<TKey>(TKey id) : CreationEntity<TKey>(id), IModification
    where TKey : IEquatable<TKey>
{
    /// <summary>
    /// 最后修改人标识
    /// </summary>
    public string LastModifierId { get; private set; }

    /// <summary>
    /// 最后修改人名称
    /// </summary>
    public string LastModifierName { get; private set; }

    /// <summary>
    /// 最后修改时间
    /// </summary>
    public DateTimeOffset? LastModificationTime { get; private set; }

    public virtual void SetModification(string lastModifierId, string lastModifierName,
        DateTimeOffset lastModificationTime = default)
    {
        LastModificationTime = lastModificationTime == default ? DateTimeOffset.Now : lastModificationTime;
        LastModifierId = lastModifierId;
        LastModifierName = lastModifierName;
    }
}
