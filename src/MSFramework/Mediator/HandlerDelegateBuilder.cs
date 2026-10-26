using System;
using System.Linq.Expressions;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;

namespace MicroserviceFramework.Mediator;

/// <summary>
/// 处理器调用委托的编译器（框架内部共享）：
/// 将 <c>HandleAsync(请求, 取消令牌)</c> 调用编译为 <c>(object handler, object request, CancellationToken) → Task</c>
/// 的强类型委托，派发期零反射调用、零 <c>object[]</c> 装箱分配。
/// Mediator（请求/命令派发）与本地事件派发（EventDescriptorStore）共用同一实现，
/// 保证相邻派发管线基建水平一致
/// </summary>
internal static class HandlerDelegateBuilder
{
    /// <summary>
    /// 为指定处理器与请求类型编译调用委托
    /// </summary>
    /// <param name="handlerType">处理器类型（闭泛型接口的实现类型）</param>
    /// <param name="method">HandleAsync 方法反射信息</param>
    /// <param name="requestType">请求/事件类型（精确闭泛型）</param>
    /// <returns>编译后的调用委托：（handler 实例, 请求/事件, 取消令牌）→ Task</returns>
    /// <exception cref="MicroserviceFrameworkException">HandleAsync 返回类型不是 <see cref="Task"/>
    /// 及其子类（如 Task&lt;TResponse&gt;）时抛出——返回 ValueTask 等其他类型时静默跳过不执行
    /// 是更隐蔽的故障形态，必须在注册期 fail-fast 而非派发期静默丢弃</exception>
    public static Func<object, object, CancellationToken, Task> Build(
        Type handlerType, MethodInfo method, Type requestType)
    {
        if (!typeof(Task).IsAssignableFrom(method.ReturnType))
        {
            throw new MicroserviceFrameworkException(
                $"处理器 {handlerType.FullName} 的 {method.Name} 返回类型必须为 Task" +
                $"（当前为 {method.ReturnType.FullName}；ValueTask 等其他类型不被派发器支持）");
        }

        var handlerParam = Expression.Parameter(typeof(object), "handler");
        var requestParam = Expression.Parameter(typeof(object), "request");
        var ctParam = Expression.Parameter(typeof(CancellationToken), "ct");

        Expression body = Expression.Call(
            Expression.Convert(handlerParam, handlerType),
            method,
            Expression.Convert(requestParam, requestType),
            ctParam);

        // 返回值若是 Task<TResponse> 则上溯到 Task，以匹配委托签名
        if (method.ReturnType != typeof(Task))
        {
            body = Expression.Convert(body, typeof(Task));
        }

        return Expression.Lambda<Func<object, object, CancellationToken, Task>>(
            body, handlerParam, requestParam, ctParam).Compile();
    }
}
