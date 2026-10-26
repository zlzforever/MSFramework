using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using MicroserviceFramework.Common;
using MicroserviceFramework.DistributedLocking;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace MicroserviceFramework.Ef.Initializer;

/// <summary>
/// EF 数据库迁移初始化器，在应用启动时对主（可写）上下文自动执行挂起的迁移。
/// 事务模型约定（与 <c>EfUnitOfWork</c> 一致）：一个应用只有一个主上下文——
/// <see cref="DbContextSettings.IsPrimary"/> 为 true 的第一个上下文，均未显式配置时回退为
/// 实体配置发现顺序的第一个；其余上下文只读，不参与迁移；
/// 多实例部署下迁移互斥由 <c>IDistributedLock</c>（如 MSFramework.Dapr）保证
/// </summary>
public class EntityFrameworkInitializerBase
    : InitializerBase
{
    private readonly IServiceProvider _serviceProvider;
    private readonly ILogger<EntityFrameworkInitializerBase> _logger;

    /// <summary>
    /// 初始化 EntityFrameworkInitializerBase 实例
    /// </summary>
    /// <param name="serviceProvider">服务提供者</param>
    /// <param name="logger">日志记录器</param>
    public EntityFrameworkInitializerBase(IServiceProvider serviceProvider,
        ILogger<EntityFrameworkInitializerBase> logger)
    {
        // 初始化器 Order 契约：值越大越先执行。
        // int.MaxValue 保证数据库迁移先于依赖表结构的业务初始化器（如数据播种）执行
        Order = int.MaxValue;
        _serviceProvider = serviceProvider;
        _logger = logger;
    }

    /// <summary>
    /// 执行数据库迁移初始化，对开启了 AutoMigrationEnabled 的 DbContext 执行挂起迁移
    /// </summary>
    /// <exception cref="MicroserviceFrameworkException">数据库迁移失败时抛出</exception>
    public override void Start()
    {
        if (Defaults.IsInTests)
        {
            return;
        }

        _logger.LogInformation("开始 EF 初始化...");
        using var scope = _serviceProvider.CreateScope();

        var list = scope.ServiceProvider.GetServices<DbContextOptions>().ToList();
        if (list.Count == 0)
        {
            _logger.LogInformation("EF 初始化结束: 未能找到数据上下文配置");
            return;
        }

        // 主上下文与 EfUnitOfWork 使用同一判定（IsPrimary 优先）；
        // 未显式配置时的回退来源也必须一致（实体配置发现顺序）——
        // 若迁移按 DI 注册顺序回退、提交按实体配置发现顺序回退，多上下文应用中
        // 两个"第一个"可能不同，出现"迁移落到 A 库、业务提交写到 B 库"的 schema 错位
        var configurationTypeFinder = scope.ServiceProvider.GetService<IEntityConfigurationTypeFinder>();
        var primaryContextType = ResolvePrimaryContextType(list, configurationTypeFinder, out var fellBackToDiscoveryOrder);
        if (list.Count > 1 && fellBackToDiscoveryOrder)
        {
            _logger.LogWarning(
                "检测到 {ContextCount} 个 DbContext 但均未显式配置 IsPrimary，" +
                "主（可写）上下文按实体配置发现顺序回退为 {PrimaryContextType}。" +
                "多上下文应用建议在 DbContexts 配置中显式指定 IsPrimary=true，避免迁移/提交目标歧义",
                list.Count, primaryContextType?.FullName);
        }

        foreach (var option in list)
        {
            var settings = option.FindExtension<DbContextSettings>();
            if (settings == null)
            {
                continue;
            }

            var dbContextType = option.ContextType;

            // 事务模型约定（与 EfUnitOfWork 一致）：只有主（可写）上下文参与迁移；
            // 其余上下文按约定只读，即使配置了 AutoMigrationEnabled 也不执行迁移
            if (dbContextType != primaryContextType && settings.AutoMigrationEnabled)
            {
                _logger.LogWarning(
                    "数据库上下文 {DbContextTypeName} 配置了自动迁移，但按框架约定它不是主（可写）上下文，已跳过迁移",
                    dbContextType.FullName);
                continue;
            }

            if (settings.AutoMigrationEnabled)
            {
                _logger.LogInformation("数据库上下文 {DbContextTypeName} 中开启了数据库自动迁移", dbContextType.FullName);

                var dbContext = (DbContext)scope.ServiceProvider.GetRequiredService(dbContextType);

                if (dbContext.Database.ProviderName == "Microsoft.EntityFrameworkCore.InMemory")
                {
                    continue;
                }

                var migrations = dbContext.Database.GetPendingMigrations().ToList();
                if (migrations.Count == 0)
                {
                    _logger.LogInformation("数据库上下文 {DbContextTypeName} 中没有挂起的迁移",
                        dbContextType.FullName);
                    continue;
                }

                // 多实例部署下互斥执行迁移：注册了 IDistributedLock（如 MSFramework.Dapr 的 UseDaprLock）
                // 则持锁执行，持锁后重新检查挂起迁移（可能已被其它实例完成）；
                // 未注册（单体部署）时直接执行、不加锁
                var distributedLock = scope.ServiceProvider.GetService<IDistributedLock>();
                if (distributedLock == null)
                {
                    dbContext.Database.Migrate();
                    _logger.LogInformation("执行了 {MigrationsCount} 个数据库迁移： {Migrations}", migrations.Count,
                        string.Join(", ", migrations));
                    continue;
                }

                var applicationInfo = scope.ServiceProvider.GetRequiredService<ApplicationInfo>();
                // 同一迁移目标的所有实例必须使用一致的应用名，才能共享迁移锁。
                using (distributedLock.Acquire($"{applicationInfo.Name}:ef-migrate:{dbContextType.FullName}", CancellationToken.None))
                {
                    // 持锁后重新检查：迁移可能已被其它实例执行完毕
                    migrations = dbContext.Database.GetPendingMigrations().ToList();
                    if (migrations.Count == 0)
                    {
                        _logger.LogInformation("数据库上下文 {DbContextTypeName} 的迁移已由其它实例完成",
                            dbContextType.FullName);
                    }
                    else
                    {
                        dbContext.Database.Migrate();
                        _logger.LogInformation("执行了 {MigrationsCount} 个数据库迁移： {Migrations}",
                            migrations.Count, string.Join(", ", migrations));
                    }
                }
            }
            else
            {
                _logger.LogInformation("数据库上下文 {DbContextTypeName} 禁用了自动迁移", dbContextType.FullName);
            }
        }

        _logger.LogInformation("EF 初始化完成");
    }

    /// <summary>
    /// 解析主（可写）上下文类型：优先取 <see cref="DbContextSettings.IsPrimary"/> 为 true 的第一个。
    /// 回退顺序与 <see cref="EfUnitOfWork"/> 保持同一来源：实体配置发现顺序的第一个，
    /// 且<b>必须与 DI 已注册上下文取交集</b>（保持发现顺序）——发现扫描可能包含未注册的上下文类型，
    /// 而工作单元按"可解析实例"过滤，不取交集会出现"业务提交正常、迁移被静默跳过"的错位；
    /// DI 注册顺序仅作两者都不可用时的最后兜底
    /// </summary>
    /// <param name="options">已注册的 DbContext 选项列表（DI 注册顺序）</param>
    /// <param name="configurationTypeFinder">实体配置查找器（实体配置发现顺序），可为 null（未注册时兜底）</param>
    /// <param name="fellBackToDiscoveryOrder">输出：是否走了回退（未显式配置 IsPrimary）</param>
    /// <returns>主上下文类型；完全无法判定时返回 null</returns>
    private static Type ResolvePrimaryContextType(IReadOnlyList<DbContextOptions> options,
        IEntityConfigurationTypeFinder configurationTypeFinder, out bool fellBackToDiscoveryOrder)
    {
        fellBackToDiscoveryOrder = true;
        foreach (var option in options)
        {
            if (option.FindExtension<DbContextSettings>() is { IsPrimary: true })
            {
                fellBackToDiscoveryOrder = false;
                return option.ContextType;
            }
        }

        // 回退 1：实体配置发现顺序的第一个，且必须与 DI 已注册上下文取交集（保持发现顺序）。
        // 发现扫描可能包含未注册（未 AddDbContext）的上下文类型；工作单元按"可解析实例"过滤，
        // 不取交集时可能选出"DI 未注册"的上下文——业务提交正常、对应迁移却被静默跳过
        if (configurationTypeFinder != null)
        {
            var registeredContextTypes = options
                .Select(o => o.ContextType)
                .ToHashSet();
            var discoveryOrderFirst = configurationTypeFinder
                .GetAllDbContextTypes()
                .FirstOrDefault(registeredContextTypes.Contains);
            if (discoveryOrderFirst != null)
            {
                return discoveryOrderFirst;
            }
        }

        // 回退 2：DI 注册顺序的第一个（仅当配置查找器不可用或交集为空时兜底）
        return options.Count > 0 ? options[0].ContextType : null;
    }
}
