﻿using System;
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
        Order = Conts.Audit;
    }

    public override async Task OnActionExecutionAsync(ActionExecutingContext context, ActionExecutionDelegate next)
    {
        _logger.LogDebug("On audit filter executing...");

        // 必须保证审计和业务用的是不同的 DbContext 不然，会导致数据异常入库
        using var scope = context.HttpContext.RequestServices.CreateScope();

        var businessUnitOfWork = context.HttpContext.RequestServices.GetService<IUnitOfWork>();
        if (businessUnitOfWork == null)
        {
            await base.OnActionExecutionAsync(context, next);
            return;
        }

        // IAuditStore auditStore = scope.ServiceProvider.GetService<IAuditStore>();
        // if (auditStore == null)
        // {
        //     await base.OnActionExecutionAsync(context, next);
        //     return;
        // }

        if (!Conts.CommandMethods.Contains(context.HttpContext.Request.Method))
        {
            await base.OnActionExecutionAsync(context, next);
            return;
        }

        var ua = context.HttpContext.Request.Headers["User-Agent"].ToString();
        var ip = context.GetRemoteIpAddress();
        var url = $"{context.HttpContext.Request.Method} {context.HttpContext.Request.GetDisplayUrl()}";
        var deviceId = context.HttpContext.Request.Query["deviceId"].ToString();
        deviceId = string.IsNullOrWhiteSpace(deviceId) ? null : deviceId;

        var deviceModel = context.HttpContext.Request.Query["deviceId"].ToString();
        deviceModel = string.IsNullOrWhiteSpace(deviceModel) ? null : deviceModel;

        double? lat = double.TryParse(context.HttpContext.Request.Query["lat"].ToString(), out var a) ? a : null;
        double? lng = double.TryParse(context.HttpContext.Request.Query["lng"].ToString(), out var n) ? n : null;
        var userId = context.HttpContext.User.Identity is { IsAuthenticated: true } and ClaimsIdentity identity
            ? identity.GetUserId()
            : string.Empty;
        var creationTime = DateTimeOffset.Now;

        businessUnitOfWork.RegisterAuditing(() =>
        {
            var auditedOperation = new AuditOperation(url, ua, ip, deviceModel, deviceId,
                lat, lng);
            auditedOperation.SetCreation(userId, creationTime);
            return auditedOperation;
        });

        await base.OnActionExecutionAsync(context, next);

        _logger.LogDebug("On audit filter executed.");

        // comment: 必须使用 HTTP request scope 的 uow manager 才能获取到审计对象
        // comment: 只有有变化的数据才会尝试获取变更对象
    }
}
