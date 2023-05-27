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

    void SetModification(string lastModifierId, string lastModifierName, DateTimeOffset lastModificationTime = default);
}
