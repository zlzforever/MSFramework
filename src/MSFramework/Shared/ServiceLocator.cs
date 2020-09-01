using System;

namespace MicroserviceFramework.Shared
{
	public class ServiceLocator
	{
		public static Func<Type, object> Provider;

		public static T Get<T>()
		{
			if (Provider == null)
			{
				throw new MicroserviceFrameworkException("服务定位没有准备好");
			}

			var service = Provider(typeof(T));
			return service == null ? default : (T) service;
		}
	}
}