using System;

namespace MSFramework
{
	public class MSFrameworkException : ApplicationException
	{
		public MSFrameworkException() : this(1, null)
		{
		}

		public MSFrameworkException(string message) : this(1, message)
		{
		}

		public MSFrameworkException(int code, string message, Exception innerException = null) : base(message,
			innerException)
		{
			Code = code;
		}

		public int Code { get; private set; }
	}
}