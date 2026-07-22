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
internal class Audit(ILogger<Audit> logger, IServiceScopeFactory scopeFactory) : ActionFilterAttribute
{
    private List<IAuditingStore> _auditingStores;
    private AuditOperation _auditOperation;
    private IServiceScope _serviceScope;

    public override async Task OnActionExecutionAsync(ActionExecutingContext context, ActionExecutionDelegate next)
    {
        logger.LogDebug("开始执行审计过滤器");

        var services = context.HttpContext.RequestServices;
        var unitOfWork = services.GetService<IUnitOfWork>();

        if (Constants.CommandMethods.Contains(context.HttpContext.Request.Method))
        {
            _serviceScope = scopeFactory.CreateScope();
            _auditingStores = _serviceScope.ServiceProvider.GetServices<IAuditingStore>().ToList();
            if (_auditingStores.Any())
            {
                _auditOperation = CreateAuditOperation(context, DateTimeOffset.UtcNow);
                unitOfWork.RegisterAuditOperation(_auditOperation);
            }
        }

        await base.OnActionExecutionAsync(context, next);
    }

    public override async Task OnResultExecutionAsync(ResultExecutingContext context, ResultExecutionDelegate next)
    {
        await base.OnResultExecutionAsync(context, next);

        if (_auditOperation != null)
        {
            _auditOperation.End();
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

        _serviceScope?.Dispose();
        logger.LogDebug("结束执行审计过滤器");
    }

    private AuditOperation CreateAuditOperation(ActionExecutingContext context, DateTimeOffset creationTime)
    {
        var ua = context.HttpContext.Request.Headers["User-Agent"].ToString();
        var ip = context.GetRemoteIpAddress();
        var url = context.HttpContext.Request.GetDisplayUrl();
        var queryString = context.HttpContext.Request.QueryString.ToString();

        var session = context.HttpContext.RequestServices.GetService<ISession>();

        var auditedOperation = new AuditOperation(url, ua, ip,
            session.GetValue(SessionField.DeviceModel),
            session.GetValue(SessionField.DeviceId),
            ParseDecimal(session.GetValue(SessionField.Latitude)),
            ParseDecimal(session.GetValue(SessionField.Longitude)),
            session.TraceIdentifier ?? context.HttpContext.TraceIdentifier,
            context.HttpContext.Request.Method)
        {
            QueryString = queryString.Length > 0 ? queryString : null,
            IMEI = session.GetValue(SessionField.IMEI),
            Platform = session.GetValue(SessionField.Platform),
            Altitude = ParseFloat(session.GetValue(SessionField.Altitude)),
            Screen = session.GetValue(SessionField.Screen),
            Battery = ParseInt(session.GetValue(SessionField.Battery)),
            Signal = ParseInt(session.GetValue(SessionField.Signal)),
            OSVersion = session.GetValue(SessionField.OSVersion),
            Accuracy = ParseFloat(session.GetValue(SessionField.Accuracy)),
            Bearing = ParseFloat(session.GetValue(SessionField.Bearing)),
            Orientation = ParseFloat(session.GetValue(SessionField.Orientation)),
            LocationSource = session.GetValue(SessionField.LocationSource),
            Emulator = ParseBool(session.GetValue(SessionField.Emulator))
        };

        auditedOperation.SetCreation(session.UserId, session.UserDisplayName, creationTime);

        return auditedOperation;
    }

    private static decimal? ParseDecimal(string value)
    {
        return decimal.TryParse(value, out var result) ? result : null;
    }

    private static float? ParseFloat(string value)
    {
        return float.TryParse(value, out var result) ? result : null;
    }

    private static int? ParseInt(string value)
    {
        return int.TryParse(value, out var result) ? result : null;
    }

    private static bool? ParseBool(string value)
    {
        return bool.TryParse(value, out var result) ? result : null;
    }
}
