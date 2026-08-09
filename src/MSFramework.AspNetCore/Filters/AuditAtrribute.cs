using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;
using MicroserviceFramework.Application;
using MicroserviceFramework.AspNetCore.Extensions;
using MicroserviceFramework.Auditing;
using MicroserviceFramework.Auditing.Model;
using MicroserviceFramework.Domain;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.Extensions;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using ISession = MicroserviceFramework.Application.ISession;

namespace MicroserviceFramework.AspNetCore.Filters;

/// <summary>
/// Audit 先于 UnitOfWork 执行，则 UnitOfWork 先于 Audit 结束（SaveChange)
/// UnitOfWork 提交完成后，则 DbContext ChangeObject 状态变清除，此时保存审计信息不会干扰业务，即便保存失败也没有关系。
/// </summary>
[AttributeUsage(AttributeTargets.Class | AttributeTargets.Method)]
internal class Audit(ILogger<Audit> logger, IServiceScopeFactory scopeFactory) : ActionFilterAttribute
{
    /// <summary>HttpContext.Items 中审计 scope 的键</summary>
    private const string AuditScopeKey = "MSFramework.Audit.Scope";
    /// <summary>HttpContext.Items 中审计存储集合的键</summary>
    private const string AuditingStoresKey = "MSFramework.Audit.Stores";
    /// <summary>HttpContext.Items 中审计操作的键</summary>
    private const string AuditOperationKey = "MSFramework.Audit.Operation";

    public override async Task OnActionExecutionAsync(ActionExecutingContext context, ActionExecutionDelegate next)
    {
        logger.LogDebug("开始执行审计过滤器");

        var httpContext = context.HttpContext;
        var unitOfWork = httpContext.RequestServices.GetService<IUnitOfWork>();

        // 未注册 IUnitOfWork 或非写操作时跳过审计
        if (unitOfWork == null || !Constants.CommandMethods.Contains(httpContext.Request.Method))
        {
            await base.OnActionExecutionAsync(context, next);
            return;
        }

        var scope = scopeFactory.CreateScope();
        try
        {
            var auditingStores = scope.ServiceProvider.GetServices<IAuditingStore>().ToList();
            if (auditingStores.Count > 0)
            {
                var auditOperation = CreateAuditOperation(context, DateTimeOffset.UtcNow, scope.ServiceProvider);
                if (auditOperation != null)
                {
                    unitOfWork.RegisterAuditOperation(auditOperation);

                    // 过滤器实例被多个请求共享，可变状态统一放入 HttpContext.Items 防止跨请求串扰
                    httpContext.Items[AuditScopeKey] = scope;
                    httpContext.Items[AuditingStoresKey] = auditingStores;
                    httpContext.Items[AuditOperationKey] = auditOperation;
                }
            }

            await base.OnActionExecutionAsync(context, next);
        }
        catch
        {
            // 业务执行异常时 OnResultExecutionAsync 不会执行，此处释放 scope 防止泄漏
            if (httpContext.Items.TryGetValue(AuditScopeKey, out var scopeItem) && scopeItem is IServiceScope leakedScope)
            {
                leakedScope.Dispose();
                httpContext.Items.Remove(AuditScopeKey);
            }

            throw;
        }
    }

    public override async Task OnResultExecutionAsync(ResultExecutingContext context, ResultExecutionDelegate next)
    {
        try
        {
            await base.OnResultExecutionAsync(context, next);
        }
        finally
        {
            var httpContext = context.HttpContext;
            try
            {
                if (httpContext.Items.TryGetValue(AuditOperationKey, out var operationItem) &&
                    operationItem is AuditOperation auditOperation &&
                    httpContext.Items.TryGetValue(AuditingStoresKey, out var storesItem) &&
                    storesItem is List<IAuditingStore> auditingStores)
                {
                    auditOperation.End();
                    foreach (var auditingStore in auditingStores)
                    {
                        try
                        {
                            await auditingStore.AddAsync(auditOperation);
                        }
                        catch (Exception ex)
                        {
                            logger.LogError(ex, "保存审计信息失败");
                        }
                    }
                }
            }
            finally
            {
                if (httpContext.Items.TryGetValue(AuditScopeKey, out var scopeItem) && scopeItem is IServiceScope scope)
                {
                    scope.Dispose();
                }

                httpContext.Items.Remove(AuditScopeKey);
                httpContext.Items.Remove(AuditingStoresKey);
                httpContext.Items.Remove(AuditOperationKey);
                logger.LogDebug("结束执行审计过滤器");
            }
        }
    }

    /// <summary>
    /// 基于请求上下文构建设备审计信息；会话或请求上下文缺失时返回 null（跳过审计）
    /// </summary>
    /// <param name="context">Action 执行上下文</param>
    /// <param name="creationTime">审计记录创建时间</param>
    /// <param name="serviceProvider">审计 scope 的服务提供程序</param>
    /// <returns>审计操作，会话缺失时返回 null</returns>
    private AuditOperation CreateAuditOperation(ActionExecutingContext context, DateTimeOffset creationTime,
        IServiceProvider serviceProvider)
    {
        var httpContext = context.HttpContext;
        var session = serviceProvider.GetService<ISession>();
        if (session == null)
        {
            logger.LogWarning("未注册 ISession，跳过审计");
            return null;
        }

        var ua = httpContext.Request.Headers["User-Agent"].ToString();
        var ip = context.GetRemoteIpAddress();
        var url = httpContext.Request.GetDisplayUrl();
        var queryString = httpContext.Request.QueryString.ToString();

        var auditedOperation = new AuditOperation(url, ua, ip,
            session.GetValue(SessionField.DeviceModel),
            session.GetValue(SessionField.DeviceId),
            ParseDecimal(session.GetValue(SessionField.Latitude)),
            ParseDecimal(session.GetValue(SessionField.Longitude)),
            session.TraceIdentifier ?? httpContext.TraceIdentifier,
            httpContext.Request.Method)
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

    /// <summary>
    /// 以不变文化解析 decimal，避免客户端区域设置影响数值格式
    /// </summary>
    private static decimal? ParseDecimal(string value)
    {
        return decimal.TryParse(value, NumberStyles.Number, CultureInfo.InvariantCulture, out var result)
            ? result
            : null;
    }

    /// <summary>
    /// 以不变文化解析 float，避免客户端区域设置影响数值格式
    /// </summary>
    private static float? ParseFloat(string value)
    {
        return float.TryParse(value, NumberStyles.Float, CultureInfo.InvariantCulture, out var result)
            ? result
            : null;
    }

    /// <summary>
    /// 以不变文化解析 int，避免客户端区域设置影响数值格式
    /// </summary>
    private static int? ParseInt(string value)
    {
        return int.TryParse(value, NumberStyles.Integer, CultureInfo.InvariantCulture, out var result)
            ? result
            : null;
    }

    /// <summary>
    /// 解析布尔值，解析失败返回 null
    /// </summary>
    private static bool? ParseBool(string value)
    {
        return bool.TryParse(value, out var result) ? result : null;
    }
}
