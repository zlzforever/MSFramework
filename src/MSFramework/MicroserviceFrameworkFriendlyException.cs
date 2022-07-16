using System;

namespace MicroserviceFramework
{
    public class MicroserviceFrameworkFriendlyException : MicroserviceFrameworkException
    {
        public MicroserviceFrameworkFriendlyException() : this(1, null)
        {
        }

        public MicroserviceFrameworkFriendlyException(string message) : this(1, message)
        {
        }

        public MicroserviceFrameworkFriendlyException(int code, string message, Exception innerException = null) : base(
            code, message,
            innerException)
        {
        }
    }
}