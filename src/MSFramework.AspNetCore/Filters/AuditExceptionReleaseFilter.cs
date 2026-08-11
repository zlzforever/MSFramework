using Microsoft.AspNetCore.Mvc.Filters;

namespace MicroserviceFramework.AspNetCore.Filters;

/// <summary>
/// 审计异常兜底过滤器：仅在 action 异常未被任何异常过滤器处理（将直接传播，N7 契约）时
/// 释放审计 scope（不保存审计），防止 scope 泄漏。
/// 异常直接传播时 MVC 在过滤器链之外重抛、结果阶段被跳过，<see cref="Audit"/> 的结果阶段
/// 保存逻辑不会执行，因此必须由本过滤器兜底释放 scope；异常已被处理（含用户自定义
/// 异常过滤器）时不做任何操作，保存与释放由 <see cref="Audit.OnResultExecutionAsync"/>
/// 统一完成。
/// </summary>
internal class AuditExceptionReleaseFilter : IExceptionFilter, IOrderedFilter
{
    /// <summary>
    /// 取最小值（<see cref="int.MinValue"/>）保证本过滤器在所有异常过滤器之后执行：
    /// MVC 异常过滤器按 Order 升序进入过滤器链、按降序（反向）执行 <see cref="OnException"/>，
    /// 本过滤器最后执行时 <see cref="ExceptionContext.ExceptionHandled"/> 才是最终结论。
    /// </summary>
    public int Order => int.MinValue;

    /// <summary>
    /// 异常未被任何异常过滤器处理（<see cref="ExceptionContext.ExceptionHandled"/> 为 false）
    /// 时释放审计 scope；异常已处理时不做任何操作。
    /// </summary>
    /// <param name="context">异常上下文，异常将直接传播时在此释放 scope</param>
    public void OnException(ExceptionContext context)
    {
        if (context.Exception != null && !context.ExceptionHandled)
        {
            Audit.ReleaseAuditScope(context.HttpContext);
        }
    }
}
