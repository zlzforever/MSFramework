using System;

namespace MSFramework.Application
{
	/// <summary>Serves as the base class for application-defined exceptions.</summary>
	public class ApplicationException : MSFrameworkException
	{
		public ApplicationException() : this(3, null)
		{
		}

		public ApplicationException(string message) : this(1, message)
		{
		}

		public ApplicationException(int code, string message, Exception innerException = null) : base(code, message,
			innerException)
		{
		}
	}
}