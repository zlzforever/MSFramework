using System;
using System.Reflection;

namespace MicroserviceFramework.EventBus
{
	public class HandlerInfo
	{
		private readonly Type _eventType;

		public HandlerInfo(Type eventType, Type handlerType, MethodInfo methodInfo)
		{
			_eventType = eventType;
			HandlerType = handlerType;
			MethodInfo = methodInfo;
		}

		public Type HandlerType { get; }
		public Type EventType => _eventType;
		public MethodInfo MethodInfo { get; }

		public override int GetHashCode()
		{
			return _eventType.GetHashCode();
		}
	}
}