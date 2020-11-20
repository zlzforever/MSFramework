using System;

namespace MicroserviceFramework.EventBus
{
	public class EventBusException : Exception
	{
		public EventBusException(string msg) : base(msg)
		{
		}
	}
}