using System;
using System.Runtime.Serialization;

namespace MSFramework.Domain
{
	public class DomainException : MSFrameworkException
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
}