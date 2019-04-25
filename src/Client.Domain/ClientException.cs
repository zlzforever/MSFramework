using System;

namespace Client.Domain
{
	public class ClientException : Exception
	{
		public ClientException(string msg) : base(msg)
		{
		}

		public ClientException(string msg, Exception e) : base(msg, e)
		{
		}
	}
}