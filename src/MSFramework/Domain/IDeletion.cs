using System;

namespace MicroserviceFramework.Domain;

public interface IDeletion : ISoftDelete
{
    /// <summary>
    /// 删除人标识
    /// </summary>
    string DeleterId { get; }

    /// <summary>
    /// 删除时间
    /// </summary>
    DateTimeOffset? DeletionTime { get; set; }

    void Delete(string userId, DateTimeOffset deletionTime = default);
}
