using System;

namespace MSFramework
{
	public class MSFrameworkException : Exception
	{
		public MSFrameworkException()
		{
		}

		public MSFrameworkException(string msg) : base(msg)
		{
		}

		public MSFrameworkException(string msg, Exception e) : base(msg, e)
		{
		}
	}
}