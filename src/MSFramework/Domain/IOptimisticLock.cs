namespace MicroserviceFramework.Domain;

/// <summary>
/// 乐观锁接口
/// </summary>
public interface IOptimisticLock
{
    /// <summary>
    /// 数据版本号, 使用数字做版本号有被撞风险
    /// </summary>
    string ConcurrencyStamp { get; set; }
}
