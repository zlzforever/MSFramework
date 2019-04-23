using System;

namespace MSFramework.EventBus
{
	public class SubscriptionInfo
	{
		public bool IsDynamic { get; }

		public Type HandlerType { get; }

		private SubscriptionInfo(bool isDynamic, Type handlerType)
		{
			IsDynamic = isDynamic;
			HandlerType = handlerType;
		}

		public static SubscriptionInfo CreateDynamic(Type handlerType)
		{
			return new SubscriptionInfo(true, handlerType);
		}

		public static SubscriptionInfo CreateTyped(Type handlerType)
		{
			return new SubscriptionInfo(false, handlerType);
		}
	}
}