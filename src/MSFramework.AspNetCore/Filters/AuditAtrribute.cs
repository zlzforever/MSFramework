using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MicroserviceFramework.Application;
using MicroserviceFramework.AspNetCore.Extensions;
using MicroserviceFramework.Auditing;
using MicroserviceFramework.Auditing.Model;
using MicroserviceFramework.Domain;
using Microsoft.AspNetCore.Http.Extensions;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace MicroserviceFramework.AspNetCore.Filters;

/// <summary>
/// Audit 先于 UnitOfWork 执行，则 UnitOfWork 先于 Audit 结束（SaveChange)
/// UnitOfWork 提交完成后，则 DbContext ChangeObject 状态变清除，此时保存审计信息不会干扰业务，即便保存失败也没有关系。
/// </summary>
[AttributeUsage(AttributeTargets.Class | AttributeTargets.Method)]
internal class Audit(ILogger<Audit> logger) : ActionFilterAttribute
{
    private List<IAuditingStore> _auditingStores;
    private AuditOperation _auditOperation;

    public override async Task OnActionExecutionAsync(ActionExecutingContext context, ActionExecutionDelegate next)
    {
        logger.LogDebug("审计过滤器执行开始");

        var services = context.HttpContext.RequestServices;
        var unitOfWork = services.GetService<IUnitOfWork>();

        if (Constants.CommandMethods.Contains(context.HttpContext.Request.Method))
        {
            _auditingStores = services.GetServices<IAuditingStore>().ToList();
            if (_auditingStores.Any())
            {
                _auditOperation = CreateAuditOperation(context, DateTimeOffset.Now);
                unitOfWork.SetAuditOperation(_auditOperation);
            }
        }

        await base.OnActionExecutionAsync(context, next);

        // comment: 必须使用 HTTP request scope 的 uow manager 才能获取到审计对象
        // comment: 只有有变化的数据才会尝试获取变更对象
    }

    public override async Task OnResultExecutionAsync(ResultExecutingContext context, ResultExecutionDelegate next)
    {
        await base.OnResultExecutionAsync(context, next);

        if (_auditOperation != null)
        {
            foreach (var auditingStore in _auditingStores)
            {
                try
                {
                    await auditingStore.AddAsync(_auditOperation);
                }
                catch (Exception ex)
                {
                    logger.LogError(ex, "保存审计信息失败");
                }
            }
        }

        logger.LogDebug("审计过滤器执行结束");
    }

    private AuditOperation CreateAuditOperation(ActionExecutingContext context, DateTimeOffset creationTime)
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
