using System;

namespace Ordering.Domain
{
	public class OrderingException : Exception
	{
		public OrderingException(string msg) : base(msg)
		{
		}

		public OrderingException(string msg, Exception e) : base(msg, e)
		{
		}
	}
}