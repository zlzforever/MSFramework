// using System;
//
// namespace MicroserviceFramework.Shared
// {
// 	/// <summary>
// 	/// 若是使用 asp.net core 定位器，则只能在 Request 的 scope 范围里获取对象。
// 	/// 即如果使用如 BackgroundService 之类的无 scope 范围的，无法通过此获得对象
// 	/// todo:
// 	/// </summary>
// 	public class ServiceLocator
// 	{
// 		private static Func<Type, object> _provider;
//
// 		public static void SetLocator(Func<Type, object> locator)
// 		{
// 			if (_provider == null)
// 			{
// 				_provider = locator;
// 			}
// 		}
//
// 		public static T Get<T>()
// 		{
// 			if (_provider == null)
// 			{
// 				throw new MicroserviceFrameworkException("服务定位没有准备好");
// 			}
//
// 			var service = Get(typeof(T));
// 			return service == null ? default : (T) service;
// 		}
//
// 		public static object Get(Type type)
// 		{
// 			return _provider(type);
// 		}
// 	}
// }