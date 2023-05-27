using System;
using System.Security.Claims;
using System.Threading.Tasks;
using MicroserviceFramework.AspNetCore.Extensions;
using MicroserviceFramework.Auditing;
using MicroserviceFramework.Domain;
using MicroserviceFramework.Security.Claims;
using Microsoft.AspNetCore.Http.Extensions;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace MicroserviceFramework.AspNetCore.Filters;

/// <summary>
///  
/// </summary>
[AttributeUsage(AttributeTargets.Class | AttributeTargets.Method)]
public class Audit : ActionFilterAttribute
{
    private readonly ILogger<Audit> _logger;

    public Audit(ILogger<Audit> logger)
    {
        _logger = logger;
    }

    public override async Task OnActionExecutionAsync(ActionExecutingContext context, ActionExecutionDelegate next)
    {
        _logger.LogDebug("审计过滤器执行开始");

        // 必须保证审计和业务用的是不同的 DbContext 不然，会导致数据异常入库
        using var scope = context.HttpContext.RequestServices.CreateScope();

        var unitOfWork = context.HttpContext.RequestServices.GetService<IUnitOfWork>();
        if (unitOfWork == null)
        {
            await base.OnActionExecutionAsync(context, next);
            return;
        }

        if (!Constants.CommandMethods.Contains(context.HttpContext.Request.Method))
        {
            await base.OnActionExecutionAsync(context, next);
            return;
        }

        var ua = context.HttpContext.Request.Headers["User-Agent"].ToString();
        var ip = context.GetRemoteIpAddress();
        var url = $"{context.HttpContext.Request.Method} {context.HttpContext.Request.GetDisplayUrl()}";
        var deviceId = context.HttpContext.Request.Query["deviceId"].ToString();
        deviceId = deviceId == string.Empty ? null : deviceId;

        var deviceModel = context.HttpContext.Request.Query["deviceModel"].ToString();
        deviceModel = deviceModel == string.Empty ? null : deviceModel;

        double? lat = double.TryParse(context.HttpContext.Request.Query["lat"].ToString(), out var a) ? a : null;
        double? lng = double.TryParse(context.HttpContext.Request.Query["lng"].ToString(), out var n) ? n : null;

        (string UserId, string UserName) user = default;
        if (context.HttpContext.User.Identity is { IsAuthenticated: true } and ClaimsIdentity identity1)
        {
            user.UserId = identity1.GetUserId();
            //user.UserName=identity1.
        }

        var userId = context.HttpContext.User.Identity is { IsAuthenticated: true } and ClaimsIdentity identity
            ? identity.GetUserId()
            : string.Empty;
        var creationTime = DateTimeOffset.Now;

        unitOfWork.RegisterAuditing(() =>
        {
            var auditedOperation = new AuditOperation(url, ua, ip, deviceModel, deviceId,
                lat, lng);
            // EF 那边可能
            auditedOperation.SetCreation(userId, "",creationTime);
            return auditedOperation;
        });

        await base.OnActionExecutionAsync(context, next);

        _logger.LogDebug("审计过滤器执行结束");

        // comment: 必须使用 HTTP request scope 的 uow manager 才能获取到审计对象
        // comment: 只有有变化的数据才会尝试获取变更对象
    }
}
