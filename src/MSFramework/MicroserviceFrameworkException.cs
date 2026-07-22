using System;

namespace MicroserviceFramework;

/// <summary>
/// MSFramework 基础异常，携带错误码和内层异常。
/// </summary>
/// <param name="code">错误码</param>
/// <param name="message">错误消息</param>
/// <param name="innerException">内部异常</param>
public class MicroserviceFrameworkException(int code, string message, Exception innerException = null)
    : ApplicationException(message,
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
    /// 异常错误码
    /// </summary>
    public int Code { get; private set; } = code;
}
