using System;

namespace MicroserviceFramework;

public class MicroserviceFrameworkException(int code, string message, Exception innerException = null)
    : ApplicationException(message,
        innerException)
{
    public MicroserviceFrameworkException() : this(1, null)
    {
    }

    public MicroserviceFrameworkException(string message) : this(1, message)
    {
    }

    public int Code { get; private set; } = code;
}
