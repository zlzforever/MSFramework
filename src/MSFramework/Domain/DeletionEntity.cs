using System;
using MongoDB.Bson;

namespace MicroserviceFramework.Domain;

/// <summary>
/// 含删除人信息的实体
/// </summary>
public abstract class DeletionEntity : DeletionEntity<ObjectId>
{
    protected DeletionEntity(ObjectId id) : base(id)
    {
    }
}

/// <summary>
/// 含删除人信息的实体
/// </summary>
public abstract class DeletionEntity<TKey> : ModificationEntity<TKey>, IDeletion where TKey : IEquatable<TKey>
{
    /// <summary>
    /// 是否已经删除
    /// </summary>
    public bool IsDeleted { get; private set; }

    /// <summary>
    /// 删除人标识
    /// </summary>
    public string DeleterId { get; private set; }

    /// <summary>
    /// 删除人
    /// </summary>
    public string DeleterName { get; private set; }

    /// <summary>
    /// 删除时间
    /// </summary>
    public DateTimeOffset? DeletionTime { get; private set; }

    public virtual void SetDeletion(string deleterId, string deleterName, DateTimeOffset deletionTime = default)
    {
        // 删除只能一次操作， 因此如果已经有值， 不能再做设置
        // 若更新成功为 true， 则不会发生再次更新的情况
        if (IsDeleted)
        {
            return;
        }

        IsDeleted = true;
        DeletionTime = deletionTime == default ? DateTimeOffset.Now : deletionTime;
        DeleterId = deleterId;
        DeleterName = deleterName;

        // DeletionTime ??= deletionTime == default ? DateTimeOffset.Now : deletionTime;
        // if (!string.IsNullOrWhiteSpace(userId) && string.IsNullOrWhiteSpace(DeleterId))
        // {
        //     DeleterId = userId;
        // }
    }

    protected DeletionEntity(TKey id) : base(id)
    {
    }
}
