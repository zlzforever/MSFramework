using System;
using MongoDB.Bson;

namespace MicroserviceFramework.Domain;

/// <summary>
/// 含删除审计信息的实体
/// </summary>
public abstract class DeletionEntity(ObjectId id) : DeletionEntity<ObjectId>(id);

/// <summary>
/// 含删除审计信息的实体
/// </summary>
public abstract class DeletionEntity<TKey>(TKey id) : ModificationEntity<TKey>(id), IDeletion
    where TKey : IEquatable<TKey>
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

    /// <summary>
    /// 设置删除审计信息
    /// </summary>
    /// <param name="deleterId">删除人标识</param>
    /// <param name="deleterName">删除人名称</param>
    /// <param name="deletionTime">删除时间</param>
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
}
