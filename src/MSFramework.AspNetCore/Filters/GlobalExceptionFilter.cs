using System;
using System.Collections.Generic;
using System.Linq;
using MicroserviceFramework.Common;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.Extensions;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.Extensions.Logging;

namespace MicroserviceFramework.AspNetCore.Filters;

/// <summary>
/// 全局异常过滤器，将未处理异常统一转换为 <see cref="ApiResult"/> 格式响应。
/// </summary>
internal class GlobalExceptionFilter(ILogger<GlobalExceptionFilter> logger) : IExceptionFilter
{
    private const string UnauthorizedErrorMessage = "无权访问";

    /// <summary>
    /// EF Core 乐观锁并发冲突异常的类型全名。
    /// 本项目不引用 EF Core（避免 ASP.NET Core 集成包反向依赖数据层），
    /// 以类型全名匹配识别；命中即映射 409
    /// </summary>
    private const string DbUpdateConcurrencyExceptionTypeName =
        "Microsoft.EntityFrameworkCore.DbUpdateConcurrencyException";

    /// <summary>
    /// 并发冲突的统一提示文案
    /// </summary>
    private const string ConcurrencyConflictMessage = "数据已被其他操作修改，请刷新后重试";

    /// <summary>
    /// 捕获异常并转换为统一响应格式：
    /// <see cref="UnauthorizedAccessException"/> → 403，
    /// <see cref="MicroserviceFrameworkFriendlyException"/> → 200 + 错误信息，
    /// EF Core <c>DbUpdateConcurrencyException</c>（乐观锁并发冲突，按类型全名识别）→ 409 + 统一提示，
    /// <see cref="OperationCanceledException"/> → 仅当 <c>RequestAborted</c> 已取消（客户端断开/
    /// 关停中止）时视为客户端取消，不构造响应仅降级记日志；内部超时等其他取消仍按 500 处理，
    /// 其他异常 → 500。
    /// </summary>
    /// <param name="context">异常过滤器上下文</param>
    public void OnException(ExceptionContext context)
    {
        if (context.ExceptionHandled)
        {
            return;
        }

        var exception = context.Exception;

        if (exception is UnauthorizedAccessException)
        {
            context.Result = new ObjectResult(new ApiResult
            {
                Success = false,
                Msg = UnauthorizedErrorMessage,
                Code = StatusCodes.Status403Forbidden,
                Data = null
            })
            {
                StatusCode = StatusCodes.Status403Forbidden
            };

            logger.LogError(exception,
                "请求 {Method} {Url} 返回 {StatusCode}",
                context.HttpContext.Request.Method,
                context.HttpContext.Request.GetDisplayUrl(),
                StatusCodes.Status403Forbidden);
        }
        else if (FindFriendlyException(exception) is { } friendlyException)
        {
            context.Result = new ObjectResult(new ApiResult
            {
                Success = false,
                Msg = friendlyException.Message,
                Code = friendlyException.Code,
                Data = null
            })
            {
                StatusCode = StatusCodes.Status200OK
            };

            logger.LogWarning(friendlyException,
                "请求 {Method} {Url} 返回 {StatusCode}",
                context.HttpContext.Request.Method,
                context.HttpContext.Request.GetDisplayUrl(),
                StatusCodes.Status200OK);
        }
        else if (FindExceptionByTypeName(exception, DbUpdateConcurrencyExceptionTypeName) != null)
        {
            // EF Core 乐观锁并发冲突（框架在修改/软删除路径轮换 ConcurrencyStamp 后会真实抛出）：
            // 属业务可预期的并发失败，映射 409 + 统一提示引导客户端刷新重试，
            // 不应混入 500 "系统内部错误"造成故障误报
            context.Result = new ObjectResult(new ApiResult
            {
                Success = false,
                Msg = ConcurrencyConflictMessage,
                Code = StatusCodes.Status409Conflict,
                Data = null
            })
            {
                StatusCode = StatusCodes.Status409Conflict
            };

            logger.LogWarning(exception,
                "请求 {Method} {Url} 乐观锁并发冲突，返回 {StatusCode}",
                context.HttpContext.Request.Method,
                context.HttpContext.Request.GetDisplayUrl(),
                StatusCodes.Status409Conflict);
        }
        else if (exception is OperationCanceledException
                 && context.HttpContext.RequestAborted.IsCancellationRequested)
        {
            // 仅当 RequestAborted 已取消时才判定为客户端取消（断开连接/关停中止请求）：
            // 非服务端故障，不再兜底为 500——那会向日志注入大量"系统内部错误"噪音告警，
            // 并把正常取消误报为服务端故障；连接已断开时响应也无法送达，不构造 Result。
            // 注意：OCE/TaskCanceledException 并非只来自客户端取消——HttpClient.Timeout、
            // CancellationTokenSource.CancelAfter 等内部超时同样抛出；
            // RequestAborted 未取消时不得降级吞掉（否则内部故障被静默化），落入下方 500 分支
            context.ExceptionHandled = true;
            logger.LogDebug(exception,
                "请求 {Method} {Url} 已被客户端取消",
                context.HttpContext.Request.Method,
                context.HttpContext.Request.GetDisplayUrl());
            return;
        }
        else
        {
            context.Result = new ObjectResult(new ApiResult
            {
                Success = false,
                Msg = "系统内部错误",
                Code = StatusCodes.Status500InternalServerError,
                Data = null
            })
            {
                StatusCode = StatusCodes.Status500InternalServerError
            };

            logger.LogError(exception,
                "请求 {Method} {Url} 返回 {StatusCode}",
                context.HttpContext.Request.Method,
                context.HttpContext.Request.GetDisplayUrl(),
                StatusCodes.Status500InternalServerError);
        }

        context.ExceptionHandled = true;
    }

    /// <summary>
    /// 深度遍历异常链：沿 InnerException 单链递归，并在 <see cref="AggregateException"/> 处
    /// 展开全部 <see cref="AggregateException.InnerExceptions"/>（Task.WhenAll 等聚合场景中，
    /// 友好异常/乐观锁并发冲突可能藏在任一内层，只走单链会漏检、导致本应 409/200 的响应被兜成 500）。
    /// 使用显式栈避免深链递归；先根后分支（内层异常在根之后返回）
    /// </summary>
    /// <param name="exception">根异常</param>
    /// <returns>异常链中全部异常（含聚合内层），按先根后分支顺序</returns>
    private static IEnumerable<Exception> Traverse(Exception exception)
    {
        var stack = new Stack<Exception>();
        stack.Push(exception);
        while (stack.Count > 0)
        {
            var current = stack.Pop();
            if (current == null)
            {
                continue;
            }

            yield return current;

            // AggregateException：InnerExceptions 已含 InnerException（通常为首个内层），
            // 不再单独压入避免重复遍历；倒序压栈保证多内层按声明顺序被访问
            if (current is AggregateException aggregate)
            {
                for (var i = aggregate.InnerExceptions.Count - 1; i >= 0; i--)
                {
                    stack.Push(aggregate.InnerExceptions[i]);
                }
            }
            else if (current.InnerException != null)
            {
                stack.Push(current.InnerException);
            }
        }
    }

    /// <summary>
    /// 深度遍历异常链按类型全名查找异常（支持任意嵌套深度的 InnerException 包装
    /// 与 <see cref="AggregateException"/> 多内层展开），未找到返回 null
    /// </summary>
    /// <param name="exception">根异常</param>
    /// <param name="typeName">异常类型的完整名称（FullName）</param>
    /// <returns>异常链中首个匹配该类型全名的异常，不存在时返回 null</returns>
    private static Exception FindExceptionByTypeName(Exception exception, string typeName)
    {
        return Traverse(exception)
            .FirstOrDefault(current => current.GetType().FullName == typeName);
    }

    /// <summary>
    /// 深度遍历异常链查找 <see cref="MicroserviceFrameworkFriendlyException"/>，
    /// 支持任意嵌套深度的 InnerException 包装与 <see cref="AggregateException"/> 多内层展开，
    /// 找不到返回 null
    /// </summary>
    /// <param name="exception">根异常</param>
    /// <returns>异常链中的友好异常，不存在时返回 null</returns>
    private static MicroserviceFrameworkFriendlyException FindFriendlyException(Exception exception)
    {
        return Traverse(exception)
            .FirstOrDefault(current => current is MicroserviceFrameworkFriendlyException)
            as MicroserviceFrameworkFriendlyException;
    }
}
