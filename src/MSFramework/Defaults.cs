using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Threading;
using MicroserviceFramework.Domain;
using MicroserviceFramework.Serialization;
using MicroserviceFramework.Text.Json;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using MongoDB.Bson;

namespace MicroserviceFramework;

/// <summary>
/// MSFramework 全局默认配置项
/// </summary>
public static class Defaults
{
    private static readonly Lazy<IJsonSerializer> DefaultJsonSerializer = new(TextJsonSerializer.Create);

    /// <summary>
    /// 分页查询的单页数量上限，默认为 500，必须大于零。
    /// 应在应用启动阶段设置；EF 分页扩展的实体和 DTO 重载均使用此值，核心包不依赖 EF。
    /// </summary>
    /// <exception cref="ArgumentOutOfRangeException">设置值小于或等于零。</exception>
    public static int MaxLimit
    {
        get;
        set
        {
            ArgumentOutOfRangeException.ThrowIfNegativeOrZero(value);
            field = value;
        }
    } = 500;

    /// <summary>
    /// 全局 JSON 序列化器实例。
    /// 未显式初始化时惰性使用默认 <see cref="TextJsonSerializer"/>。
    /// <para>
    /// 初始化契约：应只初始化一次——在应用启动阶段（Program 的 <c>UseMicroserviceFramework</c>，
    /// 或宿主在自建容器后直接设置）；二次设置会抛出异常。同进程多宿主/重复启动会互相覆盖全局配置，
    /// 导致 EF JSON ValueComparer、审计等处的序列化语义漂移；测试环境（<see cref="IsInTests"/>）豁免。
    /// </para>
    /// </summary>
    public static IJsonSerializer JsonSerializer
    {
        get => field ?? DefaultJsonSerializer.Value;
        set
        {
            ArgumentNullException.ThrowIfNull(value);
            EnsureInitializeOnce(ref _jsonSerializerInitialized, nameof(JsonSerializer));
            field = value;
        }
    }

    /// <summary>
    /// 全局日志记录器实例。
    /// 未显式初始化时惰性使用 <see cref="Microsoft.Extensions.Logging.Abstractions.NullLogger"/>。
    /// 初始化契约同 <see cref="JsonSerializer"/>：只初始化一次，二次设置抛出异常（测试环境豁免）。
    /// </summary>
    public static ILogger Logger
    {
        get => field ?? NullLogger.Instance;
        set
        {
            ArgumentNullException.ThrowIfNull(value);
            EnsureInitializeOnce(ref _loggerInitialized, nameof(Logger));
            field = value;
        }
    }

    /// <summary>
    /// 当前是否运行在测试环境中
    /// </summary>
    public static readonly bool IsInTests;

    /// <summary>
    /// 本地文件存储根目录（位于 webroot 之外）：真实文件与判重链接等本地存储统一放在
    /// <c>{应用根}/storage</c> 下，避免被静态文件中间件（默认扫描 wwwroot）按 URL 未经授权直接下载
    /// </summary>
    public static readonly string LocalStorageRoot = Path.Combine(AppContext.BaseDirectory, "storage");

    /// <summary>
    /// 本地 OSS 文件存储根目录（判重链接所在目录，位于 <see cref="LocalStorageRoot"/> 下）
    /// </summary>
    public static readonly string LocalOSSDirectory = Path.Combine(LocalStorageRoot, "oss");

    /// <summary>
    /// 全局服务提供程序，仅框架内部可覆写。
    /// 初始化契约同 <see cref="JsonSerializer"/>：只初始化一次，二次设置抛出异常（测试环境豁免）。
    /// </summary>
    public static IServiceProvider ServiceProvider
    {
        get;
        internal set
        {
            ArgumentNullException.ThrowIfNull(value);
            EnsureInitializeOnce(ref _serviceProviderInitialized, nameof(ServiceProvider));
            field = value;
        }
    }

    /// <summary>
    /// 默认项初始化标记
    /// </summary>
    private static int _jsonSerializerInitialized;

    /// <summary>
    /// 默认项初始化标记
    /// </summary>
    private static int _loggerInitialized;

    /// <summary>
    /// 默认项初始化标记
    /// </summary>
    private static int _serviceProviderInitialized;

    /// <summary>
    /// 防御同进程重复初始化：二次初始化抛出异常——多宿主/重复启动会互相覆盖全局默认项，
    /// 使序列化、审计、日志等语义在运行中漂移。测试环境（<see cref="IsInTests"/>）豁免：
    /// 测试进程会构建多个宿主，允许重复初始化。
    /// </summary>
    /// <param name="initialized">初始化标记（0 未初始化 / 1 已初始化），以 0 为初始值</param>
    /// <param name="name">默认项名称，用于异常消息</param>
    /// <exception cref="InvalidOperationException">非测试环境下二次初始化时抛出</exception>
    private static void EnsureInitializeOnce(ref int initialized, string name)
    {
        if (Interlocked.Exchange(ref initialized, 1) != 0 && !IsInTests)
        {
            throw new InvalidOperationException(
                $"Defaults.{name} 只能初始化一次：请在应用启动阶段（Program 的 UseMicroserviceFramework）完成初始化。" +
                "同进程多宿主会互相覆盖全局默认配置，如确需多宿主请拆分进程部署。");
        }
    }

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
