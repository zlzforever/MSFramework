using System;
using System.IO;
using System.Text;
using System.Threading;
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
        return await GetBodyTextAsync(httpContextAccessor, 1024 * 1024);
    }

    /// <summary>
    ///     在限制读取字节数的前提下异步读取当前 HTTP 请求体
    /// </summary>
    /// <param name="httpContextAccessor">HTTP 上下文访问器</param>
    /// <param name="maxBytes">允许读取的最大字节数</param>
    /// <param name="cancellationToken">取消令牌</param>
    /// <returns>请求体文本，若无上下文则返回 null</returns>
    /// <exception cref="InvalidDataException">请求体超过限制时抛出</exception>
    public static async Task<string> GetBodyTextAsync(
        this IHttpContextAccessor httpContextAccessor,
        int maxBytes,
        CancellationToken cancellationToken = default)
    {
        if (maxBytes <= 0)
        {
            throw new ArgumentOutOfRangeException(nameof(maxBytes));
        }

        // 缓存 HttpContext 本地变量，只读取一次
        var ctx = httpContextAccessor.HttpContext;
        if (ctx is null)
        {
            return null;
        }

        var request = ctx.Request;
        if (request.ContentLength > maxBytes)
        {
            throw new InvalidDataException($"Request body exceeds the {maxBytes}-byte limit.");
        }

        // 如果是不可 seek 的流， 说明之前没有 EnableBuffering
        if (!request.Body.CanSeek)
        {
            request.EnableBuffering();
        }

        var originalPosition = request.Body.Position;
        request.Body.Position = 0;

        try
        {
            await using var buffer = new MemoryStream(capacity: Math.Min(maxBytes, 81920));
            var chunk = new byte[Math.Min(maxBytes + 1, 81920)];
            while (buffer.Length <= maxBytes)
            {
                var remaining = maxBytes + 1 - (int)buffer.Length;
                var bytesRead = await request.Body.ReadAsync(
                    chunk.AsMemory(0, Math.Min(chunk.Length, remaining)), cancellationToken);
                if (bytesRead == 0)
                {
                    return Encoding.UTF8.GetString(buffer.GetBuffer(), 0, (int)buffer.Length);
                }

                await buffer.WriteAsync(chunk.AsMemory(0, bytesRead), cancellationToken);
            }

            throw new InvalidDataException($"Request body exceeds the {maxBytes}-byte limit.");
        }
        finally
        {
            // 归位
            request.Body.Position = originalPosition;
        }
    }
}
