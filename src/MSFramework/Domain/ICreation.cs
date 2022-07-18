using System;

namespace MicroserviceFramework.Domain;

public interface ICreation
{
    /// <summary>
    /// 创建时间
    /// </summary>
    DateTimeOffset? CreationTime { get; }

    /// <summary>
    /// 创建用户标识
    /// </summary>
    string CreatorId { get; }

    void SetCreation(string userId, DateTimeOffset creationTime = default);
}
