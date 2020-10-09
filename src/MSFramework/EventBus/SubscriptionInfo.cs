using System;
using System.Reflection;

namespace MicroserviceFramework.EventBus
{
	public class SubscriptionInfo
	{
		private readonly Type _handlerType;

		public SubscriptionInfo(Type eventType, Type handlerType, MethodInfo method)
		{
			_handlerType = handlerType;
			EventType = eventType;
			Method = method;
		}

		public Type EventType { get; private set; }
		public Type HandlerType => _handlerType;
		public MethodInfo Method { get; private set; }

		public override int GetHashCode()
		{
			return _handlerType.GetHashCode();
		}
	}
}