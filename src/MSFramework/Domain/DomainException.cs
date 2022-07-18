using System;

namespace MicroserviceFramework.Domain;

public class DomainException : MicroserviceFrameworkException
{
    public DomainException() : this(2, null)
    {
    }

    public DomainException(string message) : this(1, message)
    {
    }

    public DomainException(int code, string message, Exception innerException = null) : base(code, message,
        innerException)
    {
    }
}
