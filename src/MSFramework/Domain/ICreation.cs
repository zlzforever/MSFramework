using System;

namespace MicroserviceFramework.Domain;

public interface ICreation
{
    /// <summary>
    /// 创建时间
    /// </summary>
    DateTimeOffset? CreationTime { get; }

    /// <summary>
    /// 创建人标识
    /// </summary>
    string CreatorId { get; }

    /// <summary>
    /// 创建人名称
    /// </summary>
    string CreatorName { get; }

    void SetCreation(string creatorId, string creatorName, DateTimeOffset creationTime = default);
}
