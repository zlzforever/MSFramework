using System;

namespace MicroserviceFramework.Shared
{
	public class ServiceLocator
	{
		private static Func<Type, object> _provider;

		public static void SetLocator(Func<Type, object> locator)
		{
			_provider ??= locator;
		}

		public static T Get<T>()
		{
			if (_provider == null)
			{
				throw new MicroserviceFrameworkException("服务定位没有准备好");
			}

			var service = Get(typeof(T));
			return service == null ? default : (T) service;
		}

		public static object Get(Type type)
		{
			return _provider(type);
		}
	}
}