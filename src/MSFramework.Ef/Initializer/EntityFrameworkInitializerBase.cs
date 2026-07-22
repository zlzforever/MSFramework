using System;
using System.Linq;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace MicroserviceFramework.Ef.Initializer;

/// <summary>
/// EF 数据库迁移初始化器，在应用启动时自动执行挂起的迁移
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

        foreach (var option in list)
        {
            var settings = option.FindExtension<DbContextSettings>();
            if (settings == null)
            {
                continue;
            }

            var dbContextType = option.ContextType;
            if (settings.AutoMigrationEnabled)
            {
                _logger.LogInformation("数据库上下文 {DbContextTypeName} 中开启了数据库自动迁移", dbContextType.FullName);

                var dbContext = (DbContext)scope.ServiceProvider.GetRequiredService(dbContextType);

                if (dbContext.Database.ProviderName == "Microsoft.EntityFrameworkCore.InMemory")
                {
                    continue;
                }

                var migrations = dbContext.Database.GetPendingMigrations().ToList();
                if (migrations.Count > 0)
                {
                    dbContext.Database.Migrate();
                    _logger.LogInformation("执行了 {MigrationsCount} 个数据库迁移： {Migrations}", migrations.Count,
                        string.Join(", ", migrations));
                }
                else
                {
                    _logger.LogInformation("数据库上下文 {DbContextTypeName} 中没有挂起的迁移",
                        dbContextType.FullName);
                }
            }
            else
            {
                _logger.LogInformation("数据库上下文 {DbContextTypeName} 禁用了自动迁移", dbContextType.FullName);
            }
        }

        _logger.LogInformation("EF 初始化完成");
    }
}
