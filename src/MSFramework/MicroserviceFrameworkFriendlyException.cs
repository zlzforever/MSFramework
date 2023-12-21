using System;

namespace MicroserviceFramework;

public class MicroserviceFrameworkFriendlyException(int code, string message, Exception innerException = null)
    : MicroserviceFrameworkException(code, message,
        innerException)
{
    public MicroserviceFrameworkFriendlyException() : this(1, null)
    {
    }

    public MicroserviceFrameworkFriendlyException(string message) : this(1, message)
    {
    }
}
