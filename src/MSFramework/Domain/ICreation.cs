using System;

namespace MicroserviceFramework.Domain;

/// <summary>
/// 创建审计信息接口
/// </summary>
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

    /// <summary>
    /// 设置创建审计信息
    /// </summary>
    /// <param name="creatorId">创建人标识</param>
    /// <param name="creatorName">创建人名称</param>
    /// <param name="creationTime">创建时间</param>
    void SetCreation(string creatorId, string creatorName, DateTimeOffset creationTime = default);
}
