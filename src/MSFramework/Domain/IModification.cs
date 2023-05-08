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

    void SetModification(string lastModifierId, DateTimeOffset lastModificationTime = default);
}
