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
/// 供 DbContextBase 默认保存流程（ApplyConcepts 之后、提交之前）收集变更实体。
/// 审计仅覆盖成功请求：保存与 scope 释放统一收敛在动作阶段（<see cref="OnActionExecutionAsync"/>），
/// 只要动作阶段产生异常（无论是否被异常过滤器处理）一律不保存审计，异常记录由
/// <see cref="GlobalExceptionFilter"/> 的日志（LogWarning/LogError）承担。
/// <list type="bullet">
/// <item><description>正常完成：next 返回且 <see cref="ActionExecutedContext.Exception"/> 为空 →
/// 结束审计操作并写入全部审计存储（单点保存）；</description></item>
/// <item><description>异常被异常过滤器处理（500/403/FriendlyException 等，含用户自定义异常过滤器）：
/// next 正常返回但携带异常 → 跳过保存（契约反转，原 N5 取消）；</description></item>
/// <item><description>异常直接传播（未被任何异常过滤器处理）：next 抛出 → 跳过保存（N7 保持），
/// scope 由 catch/finally 兜底释放；</description></item>
/// <item><description>结果阶段（响应写出）异常：保存已在动作阶段完成，不受结果阶段影响，审计仍落库（不回归）；</description></item>
/// <item><description>内层过滤器短路（OnActionExecuting 设置 Result 未调用 next）：
/// <see cref="ActionExecutedContext.Exception"/> 为空 → 按成功保存，属「成功审计」边界情况，不新增处理。</description></item>
/// </list>
/// </summary>
[AttributeUsage(AttributeTargets.Class | AttributeTargets.Method)]
internal class Audit(ILogger<Audit> logger, IServiceScopeFactory scopeFactory) : ActionFilterAttribute
{
    /// <summary>HttpContext.Items 中审计 scope 的键</summary>
    private const string AuditScopeKey = "MSFramework.Audit.Scope";

    /// <summary>
    /// 审计过滤器动作阶段主流程：创建审计 scope 与审计操作，承载到 <see cref="AuditOperationContext"/>，
    /// 执行 Action（含 UnitOfWork 提交与变更实体收集），返回后单点保存审计并释放 scope。
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
            // 正常完成路径与异常路径仍能统一释放，避免 scope 泄漏
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

            // 单点保存：动作阶段结束（UnitOfWork 已提交、变更实体已收集）后统一处理。
            // next 正常返回且 Exception 为空 → 按成功保存；next 正常返回但携带异常 ⇔
            // 异常已被异常过滤器处理（全局/用户自定义），此时不保存审计（有异常一律不保存）；
            // next 抛出 ⇔ 异常直接传播，由下方 catch 兜底释放 scope，同样不保存。
            // 注意：ActionExecutingContext 本身无 Exception 属性，异常状态需从
            // next 返回的 ActionExecutedContext 读取，故直接调用 next 而非 base
            // （base 仅额外触发未使用的 OnActionExecuted 回调，行为等价）。
            var executedContext = await next();
            if (executedContext.Exception != null)
            {
                logger.LogDebug("请求存在异常（含被异常过滤器处理），跳过审计保存");
            }
            else if (auditOperation != null)
            {
                await SaveAuditOperation(auditOperation, auditingStores);
            }
        }
        catch
        {
            // 兜底：过滤器自身前置异常（scope 创建/服务解析等失败）或异常直接传播时释放 scope 防止泄漏；
            // 同时是未来框架行为变化的保险（若 .NET 后续版本逃逸路径穿透 next 抛出，此处仍兜底释放）
            ReleaseAuditScope(httpContext);

            throw;
        }
        finally
        {
            // 无论是否有异常均释放 scope：幂等释放（catch 已释放时此处不再重复释放），
            // 保存先于释放（scoped 审计存储持 DbContext 期间真实落库）
            ReleaseAuditScope(httpContext);

            // 动作阶段结束即清理执行流中的审计操作：变更实体收集发生在该阶段内的
            // UnitOfWork 提交（DbContextBase.SaveChangesAsync 在 ApplyConcepts 之后收集），
            // 此后不再需要 AsyncLocal 值
            AuditOperationContext.Value = null;
            logger.LogDebug("动作阶段结束");
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
    /// 供动作阶段正常完成路径（finally）与异常路径（catch）共用，
    /// 保证任意路径不泄漏且不重复释放。
    /// </summary>
    /// <param name="httpContext">当前请求上下文，scope 登记在 HttpContext.Items 中</param>
    internal static void ReleaseAuditScope(HttpContext httpContext)
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
