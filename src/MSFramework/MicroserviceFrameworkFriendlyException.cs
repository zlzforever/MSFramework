using System;

namespace MicroserviceFramework;

/// <summary>
/// 友好异常，用于向用户展示可理解的错误信息，避免暴露内部技术细节
/// </summary>
/// <param name="code">错误码</param>
/// <param name="message">友好错误消息</param>
/// <param name="innerException">内部异常</param>
public class MicroserviceFrameworkFriendlyException(int code, string message, Exception innerException = null)
    : MicroserviceFrameworkException(code, message,
        innerException)
{
    /// <summary>
    /// 使用默认错误码（1）创建友好异常
    /// </summary>
    public MicroserviceFrameworkFriendlyException() : this(1, null)
    {
    }

    /// <summary>
    /// 使用指定消息和默认错误码（1）创建友好异常
    /// </summary>
    /// <param name="message">友好错误消息</param>
    public MicroserviceFrameworkFriendlyException(string message) : this(1, message)
    {
    }
}
