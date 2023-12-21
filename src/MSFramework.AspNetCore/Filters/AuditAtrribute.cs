using System;
using System.Threading.Tasks;
using MicroserviceFramework.Application;
using MicroserviceFramework.AspNetCore.Extensions;
using MicroserviceFramework.Auditing.Model;
using MicroserviceFramework.Domain;
using Microsoft.AspNetCore.Http.Extensions;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace MicroserviceFramework.AspNetCore.Filters;

/// <summary>
///
/// </summary>
[AttributeUsage(AttributeTargets.Class | AttributeTargets.Method)]
internal class Audit(ILogger<Audit> logger) : ActionFilterAttribute
{
    public override async Task OnActionExecutionAsync(ActionExecutingContext context, ActionExecutionDelegate next)
    {
        logger.LogDebug("审计过滤器执行开始");

        if (!Constants.CommandMethods.Contains(context.HttpContext.Request.Method))
        {
            await base.OnActionExecutionAsync(context, next);
            return;
        }

        var services = context.HttpContext.RequestServices;

        var unitOfWork = services.GetService<IUnitOfWork>();
        if (unitOfWork == null)
        {
            await base.OnActionExecutionAsync(context, next);
            return;
        }

        var creationTime = DateTimeOffset.Now;

        unitOfWork.SetAuditOperationFactory(() => CreateAuditedOperation(context, creationTime));

        await base.OnActionExecutionAsync(context, next);

        logger.LogDebug("审计过滤器执行结束");
        // comment: 必须使用 HTTP request scope 的 uow manager 才能获取到审计对象
        // comment: 只有有变化的数据才会尝试获取变更对象
    }

    private AuditOperation CreateAuditedOperation(ActionExecutingContext context, DateTimeOffset creationTime)
    {
        var ua = context.HttpContext.Request.Headers["User-Agent"].ToString();
        var ip = context.GetRemoteIpAddress();
        var url = $"{context.HttpContext.Request.Method} {context.HttpContext.Request.GetDisplayUrl()}";
        var deviceId = context.HttpContext.Request.Query["deviceId"].ToString();
        deviceId = deviceId == string.Empty ? null : deviceId;

        var deviceModel = context.HttpContext.Request.Query["deviceModel"].ToString();
        deviceModel = deviceModel == string.Empty ? null : deviceModel;

        double? lat = double.TryParse(context.HttpContext.Request.Query["lat"].ToString(), out var a) ? a : null;
        double? lng = double.TryParse(context.HttpContext.Request.Query["lng"].ToString(), out var n) ? n : null;

        (string UserId, string UserDisplayName) user = default;
        if (context.HttpContext.User.Identity is { IsAuthenticated: true })
        {
            var session = context.HttpContext.RequestServices.GetService<ISession>();
            if (session != null)
            {
                user.UserId = session.UserId;
                user.UserDisplayName = session.UserDisplayName;
            }
        }

        var auditedOperation = new AuditOperation(url, ua, ip, deviceModel, deviceId,
            lat, lng, context.HttpContext.TraceIdentifier);
        auditedOperation.SetCreation(user.UserId, user.UserDisplayName, creationTime);
        return auditedOperation;
    }
}
