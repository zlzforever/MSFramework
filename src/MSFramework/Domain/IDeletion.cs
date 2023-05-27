using System;

namespace MicroserviceFramework.Domain;

public interface IDeletion : ISoftDelete
{
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

    void SetDeletion(string deleterId, string deleterName, DateTimeOffset deletionTime = default);
}
