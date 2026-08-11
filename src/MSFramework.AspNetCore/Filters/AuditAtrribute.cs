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
/// 审计操作由 <see cref="AuditOperationContext"/>（AsyncLocal）随执行流承载，
/// 供 DbContextBase 默认保存流程（ApplyConcepts 之后、提交之前）收集变更实体；
/// 审计保存（End + 写入存储）在 Action 执行完成后、结果阶段之前执行——
/// 结果阶段由 MVC 在调用方（ResourceInvoker）捕获的 ExecutionContext 上另行调用，
/// AsyncLocal 值不会向上传递到该执行流，因此结果阶段读不到本过滤器设置的审计操作，
/// 保存必须在 AsyncLocal 值仍有效的动作阶段完成。
/// </summary>
[AttributeUsage(AttributeTargets.Class | AttributeTargets.Method)]
internal class Audit(ILogger<Audit> logger, IServiceScopeFactory scopeFactory) : ActionFilterAttribute
{
    /// <summary>HttpContext.Items 中审计 scope 的键</summary>
    private const string AuditScopeKey = "MSFramework.Audit.Scope";

    /// <summary>
    /// 审计过滤器主流程：创建审计 scope 与审计操作，承载到 <see cref="AuditOperationContext"/>，
    /// 执行 Action（含 UnitOfWork 提交与变更实体收集），随后保存审计并清理资源。
    /// 未注册 IUnitOfWork 或非写操作时直接跳过审计。
    /// </summary>
    /// <param name="context">Action 执行上下文</param>
    /// <param name="next">Action 执行委托，调用后进入后续过滤器与 Action 本体</param>
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
            // scope 创建后无条件登记，保证未注册 IAuditingStore 或 ISession 跳过审计时，
            // 正常完成路径与异常路径仍能统一由 finally/catch 释放，避免 scope 泄漏
            httpContext.Items[AuditScopeKey] = scope;

            var auditingStores = scope.ServiceProvider.GetServices<IAuditingStore>().ToList();
            AuditOperation auditOperation = null;
            if (auditingStores.Count > 0)
            {
                auditOperation = CreateAuditOperation(context, DateTimeOffset.UtcNow, scope.ServiceProvider);
                if (auditOperation != null)
                {
                    // 审计操作承载到当前请求执行流（AsyncLocal），随 ExecutionContext 流转，
                    // 使 DbContextBase 默认保存流程（ApplyConcepts 之后、提交之前）能在同一执行流
                    // 读取到本请求的审计操作并收集变更实体
                    AuditOperationContext.Value = auditOperation;
                }
            }

            await base.OnActionExecutionAsync(context, next);

            // 保存审计必须在此处（动作阶段）完成：结果阶段（OnResultExecutionAsync）由 MVC 在
            // ResourceInvoker 捕获的 ExecutionContext 上调用，AsyncLocal 值不会向上传递到该执行流，
            // 结果阶段读不到本过滤器设置的审计操作，故保存前置到仍承载该值的动作阶段；
            // 此处使用本地引用而非重新读取 AuditOperationContext.Value：异常被异常过滤器处理后
            // 不传播（OnActionExecuted 已清理执行流），但请求仍应保存审计（与历史行为一致）
            if (auditOperation != null)
            {
                await SaveAuditOperation(auditOperation, auditingStores);
            }
        }
        catch
        {
            // 兜底：异常未走 OnActionExecuted 回调而直接传播时，此处释放 scope 防止泄漏；
            // 同时清理执行流中的审计操作，防止 AsyncLocal 值随 ExecutionContext 复用到其他请求
            ReleaseAuditScope(httpContext);
            AuditOperationContext.Value = null;

            throw;
        }
        finally
        {
            // 正常完成（含异常被异常过滤器处理后继续）路径的清理：审计保存完成后释放 scope 并
            // 清理执行流中的审计操作，防止 AsyncLocal 值随 ExecutionContext 复用到其他请求
            ReleaseAuditScope(httpContext);
            AuditOperationContext.Value = null;
            logger.LogDebug("结束执行审计过滤器");
        }
    }

    /// <summary>
    /// Action 执行结束回调。ASP.NET Core 10 中 action 异常由异常过滤器处理后不传播进
    /// <see cref="OnActionExecutionAsync"/> 的 catch，而是携带在 <paramref name="context"/>.Exception 上，
    /// 且异常短路路径下结果过滤器（OnResultExecutionAsync）不再执行，因此必须在此释放审计 scope。
    /// </summary>
    /// <param name="context">Action 执行结果上下文，异常发生时 Exception 非空</param>
    public override void OnActionExecuted(ActionExecutedContext context)
    {
        base.OnActionExecuted(context);

        if (context.Exception != null)
        {
            ReleaseAuditScope(context.HttpContext);
            // 异常短路路径下结果过滤器不再执行，此处必须同步清理执行流中的审计操作
            AuditOperationContext.Value = null;
        }
    }

    /// <summary>
    /// 结束审计操作并写入全部已注册的审计存储。
    /// 单个存储保存失败仅记录错误日志，不影响其他存储写入与请求流程。
    /// </summary>
    /// <param name="auditOperation">当前请求的审计操作（变更实体已完成收集）</param>
    /// <param name="auditingStores">已注册的审计存储集合</param>
    private async Task SaveAuditOperation(AuditOperation auditOperation, List<IAuditingStore> auditingStores)
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

    /// <summary>
    /// 幂等释放审计 scope：scope 已释放或未登记时不做任何操作。
    /// 供正常完成（OnActionExecutionAsync finally）、Action 异常（OnActionExecuted）、
    /// 异常直接传播（OnActionExecutionAsync catch）三条路径共用，保证任意路径不泄漏且不重复释放。
    /// </summary>
    /// <param name="httpContext">当前请求上下文，scope 登记在 HttpContext.Items 中</param>
    private static void ReleaseAuditScope(HttpContext httpContext)
    {
        if (httpContext.Items.TryGetValue(AuditScopeKey, out var scopeItem) && scopeItem is IServiceScope scope)
        {
            scope.Dispose();
            httpContext.Items.Remove(AuditScopeKey);
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
