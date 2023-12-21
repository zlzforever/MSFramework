using System;

namespace MicroserviceFramework.Domain;

public interface IModification
{
    /// <summary>
    /// 最后修改人标识
    /// </summary>
    string LastModifierId { get; }

    /// <summary>
    /// 最后修改时间
    /// </summary>
    DateTimeOffset? LastModificationTime { get; }

    /// <summary>
    /// 最后修改人名称
    /// </summary>
    string LastModifierName { get; }

    /// <summary>
    /// 设置最后修改信息
    /// </summary>
    /// <param name="lastModifierId">最后修改人标识</param>
    /// <param name="lastModifierName">最后修改时间</param>
    /// <param name="lastModificationTime">最后修改人名称</param>
    void SetModification(string lastModifierId, string lastModifierName,
        DateTimeOffset lastModificationTime = default);
}
