using System;
using System.Threading;
using System.Threading.Tasks;
using MicroserviceFramework.Auditing.Model;

namespace MicroserviceFramework.Domain;

/// <summary>
/// 工作单元
/// </summary>
public interface IUnitOfWork
{
    /// <summary>
    /// 设置审计信息工厂
    /// </summary>
    /// <param name="factory"></param>
    void SetAuditingFactory(Func<AuditOperation> factory);

    /// <summary>
    /// 保存工作单元
    /// </summary>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    Task SaveChangesAsync(CancellationToken cancellationToken = default);

    // void SaveChanges();

    /// <summary>
    /// 注册操作
    /// 注册的操作会在 SaveChangesAsync 发生之后执行
    /// </summary>
    /// <param name="action"></param>
    void Register(Func<Task> action);
}
