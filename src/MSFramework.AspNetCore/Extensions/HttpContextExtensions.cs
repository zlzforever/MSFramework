using System.Linq;
using System.Net;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace MicroserviceFramework.AspNetCore.Extensions;

/// <summary>
///     HttpContext 和 ActionContext 的扩展方法，提供远程 IP 地址解析
/// </summary>
public static class HttpContextExtensions
{
    /// <summary>
    ///     从 ActionContext 获取远程客户端 IP 地址字符串
    /// </summary>
    /// <param name="context">Action 上下文</param>
    /// <returns>IP 地址字符串</returns>
    public static string GetRemoteIpAddress(this ActionContext context)
    {
        return context.HttpContext.GetRemoteIpAddressString();
    }

    /// <param name="context"></param>
    extension(HttpContext context)
    {
        /// <summary>
        ///     获取远程客户端 IP 地址字符串，优先读取 X-Forwarded-For 请求头
        /// </summary>
        /// <returns>IP 地址字符串</returns>
        public string GetRemoteIpAddressString()
        {
            var forwardedFor = context.Request.Headers["X-Forwarded-For"].FirstOrDefault();
            if (string.IsNullOrEmpty(forwardedFor))
            {
                forwardedFor = context.Connection.RemoteIpAddress?.ToString();
            }

            return forwardedFor;
        }

        /// <summary>
        ///     获取远程客户端的 IPAddress 对象
        /// </summary>
        /// <returns>IPAddress 对象</returns>
        public IPAddress GetRemoteIpAddress()
        {
            var forwardedFor = context.Request.Headers["X-Forwarded-For"].FirstOrDefault();
            if (string.IsNullOrEmpty(forwardedFor))
            {
                return context.Connection.RemoteIpAddress;
            }

            return IPAddress.TryParse(forwardedFor, out var ip) ? ip : null;
        }
    }
}
