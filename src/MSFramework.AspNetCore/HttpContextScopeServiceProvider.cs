using MicroserviceFramework.Runtime;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;

namespace MicroserviceFramework.AspNetCore;

public class HttpContextScopeServiceProvider(IHttpContextAccessor httpContextAccessor)
    : ScopeServiceProvider
{
    public override T GetService<T>()
    {
        return httpContextAccessor.HttpContext == null
            ? default
            : httpContextAccessor.HttpContext.RequestServices.GetService<T>();
    }
}
