using System;
using MongoDB.Bson;

namespace MicroserviceFramework.Domain;

/// <summary>
/// 含创建人信息的实体
/// </summary>
public abstract class CreationEntity(ObjectId id) : CreationEntity<ObjectId>(id);

/// <summary>
/// 含创建人信息的实体
/// </summary>
public abstract class CreationEntity<TKey>(TKey id) :
    EntityBase<TKey>(id), ICreation
    where TKey : IEquatable<TKey>
{
    /// <summary>
    /// 创建时间
    /// </summary>
    public DateTimeOffset? CreationTime { get; private set; }

    /// <summary>
    /// 创建人标识
    /// </summary>
    public string CreatorId { get; private set; }

    /// <summary>
    /// 创建人名称
    /// </summary>
    public string CreatorName { get; private set; }

    public virtual void SetCreation(string creatorId, string creatorName, DateTimeOffset creationTime = default)
    {
        // 创建只能一次操作， 因此如果已经有值， 不能再做设置
        // 若更新成功则创建时间不会为空， 不会发生再次更新的情况
        if (CreationTime.HasValue)
        {
            return;
        }

        CreationTime = creationTime == default ? DateTimeOffset.Now : creationTime;
        CreatorId = creatorId;
        CreatorName = creatorName;
    }
}
