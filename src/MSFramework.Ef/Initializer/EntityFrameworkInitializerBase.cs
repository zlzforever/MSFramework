using System;
using System.Linq;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace MicroserviceFramework.Ef.Initializer;

/// <summary>
///
/// </summary>
public class EntityFrameworkInitializerBase
    : InitializerBase
{
    private readonly IServiceProvider _serviceProvider;
    private readonly ILogger<EntityFrameworkInitializerBase> _logger;

    /// <summary>
    ///
    /// </summary>
    /// <param name="serviceProvider"></param>
    /// <param name="logger"></param>
    public EntityFrameworkInitializerBase(IServiceProvider serviceProvider,
        ILogger<EntityFrameworkInitializerBase> logger)
    {
        Order = int.MaxValue;
        _serviceProvider = serviceProvider;
        _logger = logger;
    }

    /// <summary>
    ///
    /// </summary>
    /// <exception cref="MicroserviceFrameworkException"></exception>
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
