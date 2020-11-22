using System;
using System.Reflection;

namespace MicroserviceFramework.EventBus
{
	public class HandlerInfo
	{
		private readonly Type _handlerType;
		private readonly Type _eventType;
		private readonly MethodInfo _methodInfo;

		public HandlerInfo(Type eventType, Type handlerType, MethodInfo methodInfo)
		{
			_eventType = eventType;
			_handlerType = handlerType;
			_methodInfo = methodInfo;
		}

		public Type HandlerType => _handlerType;
		public Type EventType => _eventType;
		public MethodInfo MethodInfo => _methodInfo;

		public override int GetHashCode()
		{
			return _eventType.GetHashCode();
		}
	}
}