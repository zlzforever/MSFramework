using System.Threading;
using MicroserviceFramework.Auditing.Model;

namespace MicroserviceFramework.Auditing;

/// <summary>
/// 承载当前请求执行流审计操作的异步上下文。
/// 通过 <see cref="AsyncLocal{T}"/> 将 <see cref="AuditOperation"/> 绑定到当前执行流
/// （ExecutionContext），随 await 调用自动流转，从而把审计操作与具体的
/// <see cref="MicroserviceFramework.Domain.IUnitOfWork"/> 实现解耦：
/// 池化 DbContext 场景下，保存回调在其自身执行流中读取本上下文即可拿到属于该请求的审计操作，
/// 避免实例字段被跨请求复用导致订阅读错对象的污染问题。
/// </summary>
/// <remarks>
/// 使用约束：
/// <list type="bullet">
/// <item><description>写入时机：请求审计链路起点（如审计过滤器创建 <see cref="AuditOperation"/> 后）设置，同一执行流延续到工作单元保存。</description></item>
/// <item><description>清理时机：请求审计链路结束（审计信息保存完成后）必须将 <see cref="Value"/> 置为 null，
/// 防止值随 ExecutionContext 回收到线程池后被后续任务读到，造成跨请求串扰。</description></item>
/// <item><description>读取方（如 EF 保存回调）在 <see cref="Value"/> 为 null 时跳过审计收集，表示当前执行流无审计操作。</description></item>
/// </list>
/// </remarks>
public static class AuditOperationContext
{
    private static readonly AsyncLocal<AuditOperation> CurrentAuditOperation = new();

    /// <summary>
    /// 获取或设置当前执行流的审计操作；无审计操作时返回 null。
    /// </summary>
    /// <remarks>
    /// set 时传入 null 表示清理当前执行流的审计操作（请求结束后必须调用，
    /// 防止 AsyncLocal 值随 ExecutionContext 复用到其他请求）；get 时返回当前执行流承载的
    /// <see cref="AuditOperation"/>，未设置或已清理时返回 null。
    /// </remarks>
    public static AuditOperation Value
    {
        get => CurrentAuditOperation.Value;
        set => CurrentAuditOperation.Value = value;
    }
}
