using System;

namespace MicroserviceFramework.Domain;

public interface IDeletion
{
    /// <summary>
    /// 是否已经删除
    /// </summary>
    bool IsDeleted { get; }

    /// <summary>
    /// 删除人标识
    /// </summary>
    string DeleterId { get; }

    /// <summary>
    /// 删除人名称
    /// </summary>
    string DeleterName { get; }

    /// <summary>
    /// 删除时间
    /// </summary>
    DateTimeOffset? DeletionTime { get; }

    /// <summary>
    /// 设置删除信息
    /// </summary>
    /// <param name="deleterId">删除人标识</param>
    /// <param name="deleterName">删除人名称</param>
    /// <param name="deletionTime">删除时间</param>
    void SetDeletion(string deleterId, string deleterName, DateTimeOffset deletionTime = default);
}
