using MicroserviceFramework.Extensions.DependencyInjection;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;

namespace MicroserviceFramework.AspNetCore;

/// <summary>
///
/// </summary>
/// <param name="httpContextAccessor"></param>
public class HttpContextScopeServiceProvider(IHttpContextAccessor httpContextAccessor)
    : IScopeServiceProvider
{
    /// <summary>
    ///
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <returns></returns>
    public T GetService<T>()
    {
        return httpContextAccessor.HttpContext == null
            ? default
            : httpContextAccessor.HttpContext.RequestServices.GetService<T>();
    }
}
