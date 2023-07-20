// using System;
// using System.Collections.Generic;
// using MicroserviceFramework.Utils;
// using Microsoft.Extensions.DependencyInjection;
//
// namespace MicroserviceFramework.Extensions.DependencyInjection;
//
// /// <summary>
// /// https://odetocode.com/blogs/scott/archive/2016/02/18/avoiding-the-service-locator-pattern-in-asp-net-core.aspx
// /// 应该禁止使用服务定位器
// /// </summary>
// public static class ServiceLocator
// {
//     private static IServiceProvider _serviceProvider;
//
//     public static IServiceProvider ServiceProvider
//     {
//         get => _serviceProvider;
//         internal set
//         {
//             Check.NotNull(value, nameof(ServiceProvider));
//             _serviceProvider = value;
//         }
//     }
//
//     public static T GetService<T>(ServiceLifetime serviceLifetime = ServiceLifetime.Scoped)
//     {
//         Check.NotNull(ServiceProvider, nameof(ServiceProvider));
//         switch (serviceLifetime)
//         {
//             case ServiceLifetime.Scoped:
//             {
//                 var scopedResolver = ServiceProvider.GetRequiredService<IScopedServiceResolver>();
//                 return scopedResolver.GetService<T>();
//             }
//             case ServiceLifetime.Singleton:
//             {
//                 return ServiceProvider.GetService<T>();
//             }
//             case ServiceLifetime.Transient:
//             {
//                 return ServiceProvider.GetService<T>();
//             }
//             default:
//                 throw new ArgumentOutOfRangeException(nameof(serviceLifetime), serviceLifetime, null);
//         }
//     }
//
//     public static object GetService(Type type, ServiceLifetime serviceLifetime = ServiceLifetime.Scoped)
//     {
//         Check.NotNull(ServiceProvider, nameof(ServiceProvider));
//         switch (serviceLifetime)
//         {
//             case ServiceLifetime.Scoped:
//             {
//                 var scopedResolver = ServiceProvider.GetRequiredService<IScopedServiceResolver>();
//                 return scopedResolver.GetService(type);
//             }
//             case ServiceLifetime.Singleton:
//             {
//                 return ServiceProvider.GetService(type);
//             }
//             case ServiceLifetime.Transient:
//             {
//                 return ServiceProvider.GetService(type);
//             }
//             default:
//                 throw new ArgumentOutOfRangeException(nameof(serviceLifetime), serviceLifetime, null);
//         }
//     }
//
//     public static T GetRequiredService<T>(ServiceLifetime serviceLifetime = ServiceLifetime.Scoped)
//     {
//         Check.NotNull(ServiceProvider, nameof(ServiceProvider));
//         switch (serviceLifetime)
//         {
//             case ServiceLifetime.Scoped:
//             {
//                 var scopedResolver = ServiceProvider.GetRequiredService<IScopedServiceResolver>();
//                 var service = scopedResolver.GetService<T>();
//                 if (service == null)
//                 {
//                     throw new ArgumentNullException(nameof(service));
//                 }
//
//                 return service;
//             }
//             case ServiceLifetime.Singleton:
//             {
//                 return ServiceProvider.GetRequiredService<T>();
//             }
//             case ServiceLifetime.Transient:
//             {
//                 return ServiceProvider.GetRequiredService<T>();
//             }
//             default:
//                 throw new ArgumentOutOfRangeException(nameof(serviceLifetime), serviceLifetime, null);
//         }
//     }
//
//     public static object GetRequiredService(Type type, ServiceLifetime serviceLifetime = ServiceLifetime.Scoped)
//     {
//         Check.NotNull(ServiceProvider, nameof(ServiceProvider));
//         switch (serviceLifetime)
//         {
//             case ServiceLifetime.Scoped:
//             {
//                 var scopedResolver = ServiceProvider.GetRequiredService<IScopedServiceResolver>();
//                 var service = scopedResolver.GetService(type);
//                 if (service == null)
//                 {
//                     throw new ArgumentNullException(nameof(service));
//                 }
//
//                 return service;
//             }
//             case ServiceLifetime.Singleton:
//             {
//                 return ServiceProvider.GetRequiredService(type);
//             }
//             case ServiceLifetime.Transient:
//             {
//                 return ServiceProvider.GetRequiredService(type);
//             }
//             default:
//                 throw new ArgumentOutOfRangeException(nameof(serviceLifetime), serviceLifetime, null);
//         }
//     }
//
//     public static IEnumerable<T> GetServices<T>(ServiceLifetime serviceLifetime = ServiceLifetime.Scoped)
//     {
//         Check.NotNull(ServiceProvider, nameof(ServiceProvider));
//         switch (serviceLifetime)
//         {
//             case ServiceLifetime.Scoped:
//             {
//                 var scopedResolver = ServiceProvider.GetRequiredService<IScopedServiceResolver>();
//                 return scopedResolver.GetServices<T>();
//             }
//             case ServiceLifetime.Singleton:
//             {
//                 return ServiceProvider.GetServices<T>();
//             }
//             case ServiceLifetime.Transient:
//             {
//                 return ServiceProvider.GetServices<T>();
//             }
//             default:
//                 throw new ArgumentOutOfRangeException(nameof(serviceLifetime), serviceLifetime, null);
//         }
//     }
// }
