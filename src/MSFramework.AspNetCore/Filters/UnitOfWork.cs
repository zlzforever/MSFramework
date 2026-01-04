using System.Threading.Tasks;
using MicroserviceFramework.AspNetCore.Extensions;
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

        // TODO 每次反射 Attribute 开销太大
        if (context.HasAttribute<NoUnitOfWork>())
        {
            return;
        }

        var unitOfWork = context.HttpContext.RequestServices.GetService<IUnitOfWork>();
        if (unitOfWork == null)
        {
            return;
        }

        await unitOfWork.SaveChangesAsync();

        _logger.LogDebug("工作单元执行结束");
    }

    public int Order => Constants.UnitOfWork;
}
