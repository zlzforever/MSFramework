using MicroserviceFramework.Extensions.DependencyInjection;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;

namespace MicroserviceFramework.AspNetCore;

/// <summary>
///     基于 HttpContext 的作用域服务提供器，从当前请求中解析服务
/// </summary>
/// <param name="httpContextAccessor">HTTP 上下文访问器</param>
public class HttpContextScopeServiceProvider(IHttpContextAccessor httpContextAccessor)
    : IScopeServiceProvider
{
    /// <summary>
    ///     从当前 HTTP 请求的作用域容器中获取指定类型的服务实例
    /// </summary>
    /// <typeparam name="T">服务类型</typeparam>
    /// <returns>服务实例，若未找到则返回默认值</returns>
    public T GetService<T>()
    {
        return httpContextAccessor.HttpContext == null
            ? default
            : httpContextAccessor.HttpContext.RequestServices.GetService<T>();
    }
}
