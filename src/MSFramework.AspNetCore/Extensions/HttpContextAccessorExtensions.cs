using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;

namespace MicroserviceFramework.AspNetCore.Extensions;

/// <summary>
///
/// </summary>
public static class HttpContextAccessorExtensions
{
    /// <summary>
    ///
    /// </summary>
    /// <param name="httpContextAccessor"></param>
    /// <returns></returns>
    public static async Task<string> GetBodyTextAsync(this IHttpContextAccessor httpContextAccessor)
    {
        if (httpContextAccessor.HttpContext == null)
        {
            return null;
        }

        var request = httpContextAccessor.HttpContext.Request;
        request.EnableBuffering();
        // reader 不能释放， 会导致 body 关闭
        var reader = new System.IO.StreamReader(request.Body);
        // comments by lewis: 一定要使用异步， 同步会阻塞操作
        var text = await reader.ReadToEndAsync();
        request.Body.Position = 0;
        return text;
    }
}
