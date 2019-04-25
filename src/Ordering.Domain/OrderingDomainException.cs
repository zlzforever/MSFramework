using System;

namespace Ordering.Domain
{
	public class OrderingDomainException : Exception
	{
		public OrderingDomainException(string msg) : base(msg)
		{
		}

		public OrderingDomainException(string msg, Exception e) : base(msg, e)
		{
		}
	}
}