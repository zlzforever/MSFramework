using System;
using MongoDB.Bson;

namespace MicroserviceFramework.Domain;

/// <summary>
/// 含创建人信息的实体
/// </summary>
public abstract class CreationEntity : CreationEntity<ObjectId>
{
    protected CreationEntity(ObjectId id) : base(id)
    {
    }
}

/// <summary>
/// 含创建人信息的实体
/// </summary>
public abstract class CreationEntity<TKey> :
    EntityBase<TKey>, ICreation where TKey : IEquatable<TKey>
{
    /// <summary>
    /// 创建时间
    /// </summary>
    public DateTimeOffset? CreationTime { get; private set; }

    /// <summary>
    /// 创建人标识
    /// </summary>
    public string CreatorId { get; private set; }

    public virtual void SetCreation(string userId, DateTimeOffset creationTime = default)
    {
        // 创建只能一次操作，因此如果已经有值，不能再做设置
        CreationTime ??= creationTime == default ? DateTimeOffset.Now : creationTime;

        if (!string.IsNullOrEmpty(userId) && string.IsNullOrEmpty(CreatorId))
        {
            CreatorId = userId;
        }
    }

    protected CreationEntity(TKey id) : base(id)
    {
    }
}
