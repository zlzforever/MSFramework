using System;

namespace MicroserviceFramework;

/// <summary>
/// MSFramework 基础异常，携带错误码和内层异常。
/// </summary>
/// <param name="code">错误码</param>
/// <param name="message">错误消息</param>
/// <param name="innerException">内部异常</param>
public class MicroserviceFrameworkException(int code, string message, Exception innerException = null)
    : Exception(message,
        innerException)
{
    /// <summary>
    /// 使用错误消息创建异常，默认错误码为 1。
    /// </summary>
    /// <param name="message">错误消息</param>
    public MicroserviceFrameworkException(string message) : this(1, message)
    {
    }

    /// <summary>
    /// 使用错误消息与内部异常创建异常，默认错误码为 1。
    /// </summary>
    /// <param name="message">错误消息</param>
    /// <param name="innerException">内部异常（保留原始根因，便于排障）</param>
    public MicroserviceFrameworkException(string message, Exception innerException) : this(1, message,
        innerException)
    {
    }

    /// <summary>
    /// 异常错误码
    /// </summary>
    public int Code { get; private set; } = code;
}
