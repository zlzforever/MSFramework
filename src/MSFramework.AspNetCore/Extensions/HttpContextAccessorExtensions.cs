using System;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.WebUtilities;

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
    /// <remarks>
    ///     <para>请求体大小不做上限：网关、Dapr 等上游层已限制请求体大小，
    ///     能进入 API 的请求即认为可安全全量读取；宿主仍应配置 Kestrel/FormOptions
    ///     限制作为兜底（Kestrel 默认约 28MB）。</para>
    ///     <para>读取不可中途取消（方法不接收 CancellationToken）；读取完成后 Body 位置归位，
    ///     流不会被关闭（leaveOpen）。</para>
    ///     <para>
    ///     防静默截断：若调用前请求体已被模型绑定等管道消费（默认格式化器不缓冲、不可 seek），
    ///     本方法开启的缓冲只能覆盖"此后"的内容。回读完成后会以缓冲流已缓冲字节数与
    ///     ContentLength 对账，不一致时抛出 <see cref="InvalidOperationException"/> 而不是
    ///     返回残缺数据——静默截断会让审计/日志拿到损坏的请求体。无 ContentLength
    ///     （chunked 传输）时无法对账，属已知限制，调用方应尽量在模型绑定前
    ///     （如中间件）提前调用 <c>request.EnableBuffering()</c>。
    ///     </para>
    /// </remarks>
    /// <exception cref="InvalidOperationException">请求体已包装缓冲但回读字节数与 ContentLength
    /// 不一致（请求体此前已被部分消费，回读内容不完整）时抛出</exception>
    public static async Task<string> GetBodyTextAsync(this IHttpContextAccessor httpContextAccessor)
    {
        // 缓存 HttpContext 本地变量，只读取一次
        var ctx = httpContextAccessor.HttpContext;
        if (ctx is null)
        {
            return null;
        }

        var request = ctx.Request;
        // 记录包装前是否已可 seek：可 seek 说明此前已开启缓冲（本方法重复调用或调用方提前开启），
        // 缓冲内容完整，无需事后对账
        var wasSeekable = request.Body.CanSeek;
        if (!wasSeekable)
        {
            request.EnableBuffering();
        }

        var originalPosition = request.Body.Position;
        request.Body.Position = 0;

        try
        {
            // leaveOpen: true 保证 StreamReader.Dispose 不关闭 Body 流（此前"reader 不能释放"
            // 的说法有误，错误机制恰是 leaveOpen）
            string text;
            using (var reader = new StreamReader(request.Body, Encoding.UTF8, true, 1024, leaveOpen: true))
            {
                // comments by lewis: 一定要使用异步， 同步会阻塞操作
                text = await reader.ReadToEndAsync();
            }

            // 截断对账：缓冲流已缓冲字节数即本次回读读到的内容；与 ContentLength 不符说明
            // 包装缓冲前请求体已被部分消费，回读内容不完整——抛出而非返回残缺数据
            if (!wasSeekable && request.ContentLength is long expected &&
                request.Body is FileBufferingReadStream buffering &&
                buffering.Length != expected)
            {
                throw new InvalidOperationException(
                    "请求体已被部分消费且未开启缓冲，回读内容不完整（已缓冲 " +
                    $"{buffering.Length} 字节，ContentLength 为 {expected} 字节）。" +
                    "请在模型绑定之前（如中间件）调用 GetBodyTextAsync，" +
                    "或提前调用 request.EnableBuffering() 开启缓冲");
            }

            return text;
        }
        finally
        {
            // 归位
            request.Body.Position = originalPosition;
        }
    }
}
