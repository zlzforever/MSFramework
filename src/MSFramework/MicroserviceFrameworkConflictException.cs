using System;

namespace MicroserviceFramework;

/// <summary>
/// 表示请求与资源当前状态发生明确冲突的业务异常。
/// 只有此异常才应由 ASP.NET Core 全局异常过滤器转换为 HTTP 409。
/// </summary>
/// <param name="code">错误码</param>
/// <param name="message">冲突说明</param>
/// <param name="innerException">内部异常</param>
public class MicroserviceFrameworkConflictException(int code, string message, Exception innerException = null)
    : MicroserviceFrameworkException(code, message, innerException)
{
    /// <summary>
    /// 使用默认错误码（1）创建冲突异常。
    /// </summary>
    /// <param name="message">冲突说明</param>
    public MicroserviceFrameworkConflictException(string message) : this(1, message)
    {
    }
}
