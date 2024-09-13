using System;

namespace MicroserviceFramework;

/// <summary>
///
/// </summary>
/// <param name="code"></param>
/// <param name="message"></param>
/// <param name="innerException"></param>
public class MicroserviceFrameworkFriendlyException(int code, string message, Exception innerException = null)
    : MicroserviceFrameworkException(code, message,
        innerException)
{
    /// <summary>
    ///
    /// </summary>
    public MicroserviceFrameworkFriendlyException() : this(1, null)
    {
    }

    /// <summary>
    ///
    /// </summary>
    /// <param name="message"></param>
    public MicroserviceFrameworkFriendlyException(string message) : this(1, message)
    {
    }
}
