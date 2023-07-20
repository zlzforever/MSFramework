// using System;
// using System.Collections.Generic;
//
// namespace MicroserviceFramework.Extensions.DependencyInjection;
//
// /// <summary>
// /// 因为服务器定位模型应该禁止使用，此接口作为支撑接口无存在必要
// /// </summary>
// public interface IScopedServiceResolver
// {
//     /// <summary>
//     /// 获取指定服务类型的实例
//     /// </summary>
//     /// <typeparam name="T">服务类型</typeparam>
//     /// <returns></returns>
//     T GetService<T>();
//
//     /// <summary>
//     /// 获取指定服务类型的实例
//     /// </summary>
//     /// <param name="serviceType">服务类型</param>
//     /// <returns></returns>
//     object GetService(Type serviceType);
//
//     /// <summary>
//     /// 获取指定服务类型的所有实例
//     /// </summary>
//     /// <typeparam name="T">服务类型</typeparam>
//     /// <returns></returns>
//     IEnumerable<T> GetServices<T>();
//
//     /// <summary>
//     /// 获取指定服务类型的所有实例
//     /// </summary>
//     /// <param name="serviceType">服务类型</param>
//     /// <returns></returns>
//     IEnumerable<object> GetServices(Type serviceType);
// }
