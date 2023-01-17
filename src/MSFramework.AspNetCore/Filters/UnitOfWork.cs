using System.Threading.Tasks;
using MicroserviceFramework.AspNetCore.Extensions;
using MicroserviceFramework.Domain;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace MicroserviceFramework.AspNetCore.Filters;

public class UnitOfWork : IAsyncActionFilter, IOrderedFilter
{
    private readonly ILogger _logger;

    public UnitOfWork(ILogger<UnitOfWork> logger)
    {
        _logger = logger;
    }

    public async Task OnActionExecutionAsync(ActionExecutingContext context, ActionExecutionDelegate next)
    {
        _logger.LogDebug("On unit of work filter executing...");

        var result = await next();

        // 若有异常，不应该提交数据
        if (result.Exception != null)
        {
            return;
        }

        if (context.HasAttribute<NoneUnitOfWork>())
        {
            return;
        }

        var unitOfWork = context.HttpContext.RequestServices.GetService<IUnitOfWork>();
        if (unitOfWork == null)
        {
            return;
        }

        await unitOfWork.SaveChangesAsync();

        _logger.LogDebug("On unit of work filter executed.");
    }

    public int Order => Constants.UnitOfWork;
}
