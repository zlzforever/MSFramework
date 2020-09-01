using System;

namespace MicroserviceFramework
{
	public class MicroserviceFrameworkException : ApplicationException
	{
		public MicroserviceFrameworkException() : this(1, null)
		{
		}

		public MicroserviceFrameworkException(string message) : this(1, message)
		{
		}

		public MicroserviceFrameworkException(int code, string message, Exception innerException = null) : base(message,
			innerException)
		{
			Code = code;
		}

		public int Code { get; private set; }
	}
}