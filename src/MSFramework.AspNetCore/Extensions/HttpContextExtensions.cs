using System.Linq;
using System.Net;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace MicroserviceFramework.AspNetCore.Extensions;

/// <summary>
///
/// </summary>
public static class HttpContextExtensions
{
    /// <summary>
    ///
    /// </summary>
    /// <param name="context"></param>
    /// <returns></returns>
    public static string GetRemoteIpAddress(this ActionContext context)
    {
        return context.HttpContext.GetRemoteIpAddressString();
    }

    /// <summary>
    ///
    /// </summary>
    /// <param name="context"></param>
    /// <returns></returns>
    public static string GetRemoteIpAddressString(this HttpContext context)
    {
        var forwardedFor = context.Request.Headers["X-Forwarded-For"].FirstOrDefault();
        if (string.IsNullOrEmpty(forwardedFor))
        {
            forwardedFor = context.Connection.RemoteIpAddress?.ToString();
        }

        return forwardedFor;
    }

    /// <summary>
    ///
    /// </summary>
    /// <param name="context"></param>
    /// <returns></returns>
    public static IPAddress GetRemoteIpAddress(this HttpContext context)
    {
        var forwardedFor = context.Request.Headers["X-Forwarded-For"].FirstOrDefault();
        if (string.IsNullOrEmpty(forwardedFor))
        {
            return context.Connection.RemoteIpAddress;
        }

        return IPAddress.TryParse(forwardedFor, out var ip) ? ip : null;
    }
}
