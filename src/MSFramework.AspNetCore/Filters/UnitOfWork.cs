using System.Threading.Tasks;
using MicroserviceFramework.Domain;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace MicroserviceFramework.AspNetCore.Filters;

internal class UnitOfWork(ILogger<UnitOfWork> logger) : IAsyncActionFilter, IOrderedFilter
{
    private readonly ILogger _logger = logger;

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

        var unitOfWork = context.HttpContext.RequestServices.GetService<IUnitOfWork>();
        if (unitOfWork == null)
        {
            return;
        }

        await unitOfWork.SaveChangesAsync();

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
