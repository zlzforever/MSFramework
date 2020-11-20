// using System;
// using MicroserviceFramework.DependencyInjection;
// using Microsoft.Extensions.DependencyInjection;
//
// namespace MicroserviceFramework.Shared
// {
// 	/// <summary>
// 	/// 若是使用 asp.net core 定位器，则只能在 Request 的 scope 范围里获取对象。
// 	/// 即如果使用如 BackgroundService 之类的无 scope 范围的，无法通过此获得对象
// 	/// todo:
// 	/// </summary>
// 	public class ServiceLocator : IScopeDependency
// 	{
// 		private readonly IServiceProvider _serviceProvider;
// 		private static IServiceProvider _rootServiceProvider;
//
// 		public ServiceLocator(IServiceProvider serviceProvider)
// 		{
// 			_serviceProvider = serviceProvider;
// 		}
//
// 		public ServiceLocator Root => new ServiceLocator(_rootServiceProvider);
//
// 		public static void SetLocator(IServiceProvider serviceProvider)
// 		{
// 			_rootServiceProvider = serviceProvider;
// 		}
//
// 		public T GetService<T>()
// 		{
// 			if (_serviceProvider == null)
// 			{
// 				throw new MicroserviceFrameworkException("服务定位没有准备好");
// 			}
//
// 			var service = _serviceProvider.GetService<T>();
// 			return service == null ? default : service;
// 		}
// 	}
// }