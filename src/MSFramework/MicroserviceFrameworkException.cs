using System;

namespace MicroserviceFramework;

/// <summary>
///
/// </summary>
/// <param name="code"></param>
/// <param name="message"></param>
/// <param name="innerException"></param>
public class MicroserviceFrameworkException(int code, string message, Exception innerException = null)
    : ApplicationException(message,
        innerException)
{
    /// <summary>
    ///
    /// </summary>
    public MicroserviceFrameworkException() : this(1, null)
    {
    }

    /// <summary>
    ///
    /// </summary>
    /// <param name="message"></param>
    public MicroserviceFrameworkException(string message) : this(1, message)
    {
    }

    /// <summary>
    ///
    /// </summary>
    public int Code { get; private set; } = code;
}
