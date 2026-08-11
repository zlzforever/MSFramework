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
/// 审计保存（End + 写入存储）统一延迟到结果阶段完成（异常被异常过滤器处理时经
/// <see cref="IAsyncAlwaysRunResultFilter"/> 路径同样执行），保证：
/// <list type="bullet">
/// <item><description>保存时审计 scope 仍存活（scoped 审计存储不会被提前 Dispose，异常路径审计真实落库，N6）；</description></item>
/// <item><description>异常直接传播（未被任何异常过滤器处理）时结果阶段被跳过、不保存审计（N7）；</description></item>
/// <item><description>异常被异常过滤器处理后（含用户自定义异常过滤器）仍保存审计（N5）。</description></item>
/// </list>
/// </summary>
[AttributeUsage(AttributeTargets.Class | AttributeTargets.Method)]
internal class Audit(ILogger<Audit> logger, IServiceScopeFactory scopeFactory)
    : ActionFilterAttribute, IAsyncAlwaysRunResultFilter
{
    /// <summary>HttpContext.Items 中审计 scope 的键</summary>
    private const string AuditScopeKey = "MSFramework.Audit.Scope";

    /// <summary>HttpContext.Items 中审计保存状态（审计操作 + 已解析的审计存储）的键</summary>
    private const string AuditSaveStateKey = "MSFramework.Audit.SaveState";

    /// <summary>
    /// 审计过滤器动作阶段主流程：创建审计 scope 与审计操作，承载到 <see cref="AuditOperationContext"/>，
    /// 执行 Action（含 UnitOfWork 提交与变更实体收集）。保存与 scope 释放不在本阶段进行——
    /// 统一延迟到结果阶段（<see cref="OnResultExecutionAsync"/>）与异常阶段（<see cref="AuditExceptionReleaseFilter"/>），
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

                    // 审计保存状态跨阶段传递（动作阶段 → 结果阶段/异常阶段）：
                    // 过滤器实例生命周期不保证跨阶段安全，统一放入 HttpContext.Items
                    httpContext.Items[AuditSaveStateKey] = new AuditSaveState(auditOperation, auditingStores);
                }
            }

            await base.OnActionExecutionAsync(context, next);
        }
        catch
        {
            // 兜底：过滤器自身前置异常（scope 创建/服务解析等失败）直接传播时释放 scope 防止泄漏
            ReleaseAuditScope(httpContext);

            throw;
        }
        finally
        {
            // 动作阶段结束即清理执行流中的审计操作：变更实体收集发生在该阶段内的
            // UnitOfWork 提交（DbContextBase.SaveChangesAsync 在 ApplyConcepts 之后收集），
            // 此后结果阶段/异常阶段在 ResourceInvoker 捕获的 ExecutionContext 上执行，
            // 不再需要 AsyncLocal 值；此处不释放审计 scope——保存延迟到结果阶段
            // （OnResultExecutionAsync），scope 必须存活到保存完成
            AuditOperationContext.Value = null;
            logger.LogDebug("动作阶段结束");
        }
    }

    /// <summary>
    /// 审计过滤器结果阶段：保存审计并释放审计 scope。
    /// 到达结果阶段 ⇔ 请求不存在未处理异常——异常直接传播时 MVC 在过滤器链之外重抛、
    /// 结果阶段被跳过（故此处保存天然满足「异常直接传播不保存」契约，N7）；
    /// 异常被异常过滤器处理后 MVC 走 <see cref="IAlwaysRunResultFilter"/> 路径，
    /// 本过滤器实现 <see cref="IAsyncAlwaysRunResultFilter"/> 保证该路径仍执行到此处（N5 保存路径）。
    /// 保存发生在 finally 中：无论结果写出成功、取消或抛出异常，scope 均被释放且仅释放一次。
    /// 异常直接传播时 scope 由 <see cref="AuditExceptionReleaseFilter"/> 释放（不保存审计）。
    /// </summary>
    /// <param name="context">结果执行上下文</param>
    /// <param name="next">结果执行委托，调用后写出响应</param>
    public override async Task OnResultExecutionAsync(ResultExecutingContext context, ResultExecutionDelegate next)
    {
        try
        {
            await base.OnResultExecutionAsync(context, next);
        }
        finally
        {
            // 保存先于 scope 释放（N6 时序）：scoped 审计存储（默认 EfAuditingStore 持 DbContext）
            // 在 scope 存活期间调用 AddAsync，异常路径审计真实落库
            var httpContext = context.HttpContext;
            if (httpContext.Items.TryGetValue(AuditSaveStateKey, out var saveStateItem) &&
                saveStateItem is AuditSaveState saveState)
            {
                await SaveAuditOperation(saveState.AuditOperation, saveState.AuditingStores);
                httpContext.Items.Remove(AuditSaveStateKey);
            }

            ReleaseAuditScope(httpContext);
            logger.LogDebug("结束执行审计过滤器");
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
    /// 供结果阶段（<see cref="OnResultExecutionAsync"/> finally）、异常直接传播
    /// （<see cref="AuditExceptionReleaseFilter"/>）、过滤器前置异常
    /// （<see cref="OnActionExecutionAsync"/> catch）三条路径共用，
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

    /// <summary>
    /// 审计保存跨阶段（动作阶段 → 结果阶段/异常阶段）传递的状态：
    /// 承载待保存的审计操作与已从审计 scope 解析的审计存储。
    /// </summary>
    /// <param name="AuditOperation">待保存的审计操作</param>
    /// <param name="AuditingStores">已解析的审计存储集合</param>
    private sealed record AuditSaveState(AuditOperation AuditOperation, List<IAuditingStore> AuditingStores);
}
