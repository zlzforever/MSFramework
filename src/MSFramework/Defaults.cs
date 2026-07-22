using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using MicroserviceFramework.Domain;
using MicroserviceFramework.Serialization;
using Microsoft.Extensions.Logging;
using MongoDB.Bson;

namespace MicroserviceFramework;

/// <summary>
/// MSFramework 全局默认配置项
/// </summary>
public static class Defaults
{
    /// <summary>
    /// 全局 JSON 序列化器实例
    /// </summary>
    public static IJsonSerializer JsonSerializer;

    /// <summary>
    /// 全局日志记录器实例
    /// </summary>
    public static ILogger Logger;

    /// <summary>
    /// 当前是否运行在测试环境中
    /// </summary>
    public static readonly bool IsInTests;

    /// <summary>
    /// 本地 OSS 文件存储根目录
    /// </summary>
    public static readonly string LocalOSSDirectory = Path.Combine(AppContext.BaseDirectory, "wwwroot", "oss");

    /// <summary>
    /// 全局服务提供程序
    /// </summary>
    public static IServiceProvider ServiceProvider;

    static Defaults()
    {
        List<string> names = ["ReSharperTestRunner", "testhost"];
        IsInTests = names.Contains(Assembly.GetEntryAssembly()?.GetName().Name);
    }

    /// <summary>
    /// HTTP 请求头常量
    /// </summary>
    public static class Headers
    {
        /// <summary>
        /// 内部调用标识请求头
        /// </summary>
        public const string InternalCall = "Internal-Call";
    }

    /// <summary>
    /// 常用类型静态引用
    /// </summary>
    public static class Types
    {
        /// <summary>
        /// <see cref="string"/> 类型引用
        /// </summary>
        public static readonly Type String = typeof(string);

        /// <summary>
        /// <see cref="Guid"/> 类型引用
        /// </summary>
        public static readonly Type Guid = typeof(Guid);

        /// <summary>
        /// <see cref="MongoDB.Bson.ObjectId"/> 类型引用
        /// </summary>
        public static readonly Type ObjectId = typeof(ObjectId);

        /// <summary>
        /// <see cref="IOptimisticLock"/> 接口类型引用
        /// </summary>
        public static readonly Type OptimisticLock = typeof(IOptimisticLock);

        /// <summary>
        /// <see cref="IEntity"/> 接口类型引用
        /// </summary>
        public static readonly Type Entity = typeof(IEntity);

        /// <summary>
        /// <see cref="IExternalEntity"/> 接口类型引用
        /// </summary>
        public static readonly Type ExternalEntity = typeof(IExternalEntity);

        /// <summary>
        /// <see cref="IRepository"/> 接口类型引用
        /// </summary>
        public static readonly Type Repository = typeof(IRepository);

        /// <summary>
        /// <see cref="ValueObject"/> 类型引用
        /// </summary>
        public static readonly Type ValueObject = typeof(ValueObject);
    }
}
