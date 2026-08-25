using System.IO;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;

namespace MicroserviceFramework.AspNetCore.Extensions;

/// <summary>
///     IHttpContextAccessor 的扩展方法，提供 HTTP 请求体读取能力
/// </summary>
public static class HttpContextAccessorExtensions
{
    /// <summary>
    ///     异步读取当前 HTTP 请求的 Body 内容为字符串
    /// </summary>
    /// <param name="httpContextAccessor">HTTP 上下文访问器</param>
    /// <returns>请求体文本，若无上下文则返回 null</returns>
    public static async Task<string> GetBodyTextAsync(this IHttpContextAccessor httpContextAccessor)
    {
        // 缓存 HttpContext 本地变量，只读取一次
        var ctx = httpContextAccessor.HttpContext;
        if (ctx is null)
        {
            return null;
        }

        var request = ctx.Request;
        // 如果是不可 seek 的流， 说明之前没有 EnableBuffering
        if (!request.Body.CanSeek)
        {
            request.EnableBuffering();
        }

        var originalPosition = request.Body.Position;
        request.Body.Position = 0;

        try
        {
            // reader 不能释放， 会导致 body 关闭
            using var reader = new StreamReader(
                request.Body, Encoding.UTF8, true, 1024, leaveOpen: true);
            // comments by lewis: 一定要使用异步， 同步会阻塞操作
            var text = await reader.ReadToEndAsync();
            return text;
        }
        finally
        {
            // 归位
            request.Body.Position = originalPosition;
        }
    }
}
