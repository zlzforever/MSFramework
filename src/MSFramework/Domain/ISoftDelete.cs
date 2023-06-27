namespace MicroserviceFramework.Domain;

/// <summary>
/// 软删除接口
/// </summary>
public interface ISoftDelete
{
    /// <summary>
    /// 是否已经删除
    /// </summary>
    bool IsDeleted { get; }
}
