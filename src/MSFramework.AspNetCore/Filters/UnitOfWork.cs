using System.Threading.Tasks;
using MicroserviceFramework.Domain;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace MicroserviceFramework.AspNetCore.Filters;

/// <summary>
/// 工作单元过滤器，Action 成功执行后自动提交 <see cref="IUnitOfWork"/>。
/// 仅对 HTTP 写方法（POST/PUT/PATCH/DELETE）提交，只读方法不提交。
/// 可通过 <see cref="NoUnitOfWork"/> Attribute 跳过提交。
/// </summary>
internal class UnitOfWork(ILogger<UnitOfWork> logger) : IAsyncActionFilter, IOrderedFilter
{
    private readonly ILogger _logger = logger;

    /// <summary>
    /// Action 执行后若无异常且未标记 <see cref="NoUnitOfWork"/>，自动提交工作单元。
    /// </summary>
    /// <param name="context">当前动作上下文，提交时传递其 RequestAborted 令牌。</param>
    /// <param name="next">后续动作委托；动作存在异常时不提交。</param>
    /// <returns>动作及工作单元提交完成时完成的任务。</returns>
    /// <exception cref="System.OperationCanceledException">请求取消且工作单元响应取消时抛出。</exception>
    public async Task OnActionExecutionAsync(ActionExecutingContext context, ActionExecutionDelegate next)
    {
        _logger.LogDebug("开始执行工作单元过滤器");

        var result = await next();

        // 若有异常，不应该提交数据
        if (result.Exception != null)
        {
            return;
        }

        // 通过 EndpointMetadata 检查 [NoUnitOfWork]，零反射零分配
        // EndpointMetadata 由 ASP.NET Core 在启动时构建，包含 Controller 和 Action 上的所有 Attribute 实例
        if (HasNoUnitOfWork(context))
        {
            return;
        }

        // 仅写方法（POST/PUT/PATCH/DELETE）提交，与审计过滤器的写方法判定同口径：
        // 1) 只读请求省去解析 IUnitOfWork 与一次变更检测的开销；
        // 2) 防止查询过程中对被跟踪实体的隐式修改在 GET 请求里被静默提交
        //    （隐藏写操作 + 审计只记写方法导致的审计缺漏）
        if (!Constants.CommandMethods.Contains(context.HttpContext.Request.Method))
        {
            return;
        }

        var unitOfWork = context.HttpContext.RequestServices.GetService<IUnitOfWork>();
        if (unitOfWork == null)
        {
            return;
        }

        await unitOfWork.SaveChangesAsync(context.HttpContext.RequestAborted);

        _logger.LogDebug("结束执行工作单元过滤器");
    }

    public int Order => Constants.UnitOfWork;

    private static bool HasNoUnitOfWork(ActionExecutingContext context)
    {
        var metadata = context.ActionDescriptor.EndpointMetadata;
        for (var i = 0; i < metadata.Count; i++)
        {
            if (metadata[i] is NoUnitOfWork)
            {
                return true;
            }
        }

        return false;
    }
}
