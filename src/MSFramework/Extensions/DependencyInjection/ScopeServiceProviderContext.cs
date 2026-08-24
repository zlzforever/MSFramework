using System;
using System.Threading;

namespace MicroserviceFramework.Extensions.DependencyInjection;

/// <summary>
/// 当前异步执行流的作用域服务提供器。
/// 用于没有 HTTP 上下文的后台任务，让基础设施代码仍能解析当前事件作用域中的服务。
/// </summary>
public static class ScopeServiceProviderContext
{
    private static readonly AsyncLocal<IScopeServiceProvider> CurrentProvider = new();

    /// <summary>
    /// 当前异步执行流的作用域服务提供器。
    /// </summary>
    public static IScopeServiceProvider Current
    {
        get => CurrentProvider.Value;
        private set => CurrentProvider.Value = value;
    }

    /// <summary>
    /// 设置当前作用域服务提供器，并在释放返回的句柄时恢复上一个值。
    /// </summary>
    /// <param name="provider">当前作用域服务提供器</param>
    /// <returns>恢复上一个作用域的句柄</returns>
    public static IDisposable Push(IScopeServiceProvider provider)
    {
        var previous = Current;
        Current = provider;
        return new RestoreScope(previous);
    }

    private sealed class RestoreScope(IScopeServiceProvider previous) : IDisposable
    {
        public void Dispose()
        {
            Current = previous;
        }
    }
}
