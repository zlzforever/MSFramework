using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace MicroserviceFramework.Ef.Initializer;

/// <summary>
///
/// </summary>
public class EntityFrameworkInitializer
    : MicroserviceFramework.Initializer
{
    private readonly IServiceProvider _serviceProvider;
    private readonly ILogger<EntityFrameworkInitializer> _logger;

    /// <summary>
    ///
    /// </summary>
    /// <param name="serviceProvider"></param>
    /// <param name="logger"></param>
    public EntityFrameworkInitializer(IServiceProvider serviceProvider, ILogger<EntityFrameworkInitializer> logger)
    {
        Order = int.MaxValue;
        Synchronized = true;
        _serviceProvider = serviceProvider;
        _logger = logger;
    }

    /// <summary>
    ///
    /// </summary>
    /// <param name="cancellationToken"></param>
    /// <exception cref="MicroserviceFrameworkException"></exception>
    public override async Task StartAsync(CancellationToken cancellationToken)
    {
        if (Defaults.IsInTests)
        {
            return;
        }

        try
        {
            _logger.LogInformation("开始 EF 初始化...");
            using var scope = _serviceProvider.CreateScope();

            var list = scope.ServiceProvider.GetServices<DbContextOptions>().ToList();
            if (list.Count == 0)
            {
                throw new MicroserviceFrameworkException("未能找到数据上下文配置");
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

                    var migrations =
                        (await dbContext.Database.GetPendingMigrationsAsync(cancellationToken: cancellationToken))
                        .ToArray();
                    if (migrations.Length > 0)
                    {
                        await dbContext.Database.MigrateAsync(cancellationToken: cancellationToken);

                        _logger.LogInformation("执行了 {MigrationsCount} 个数据库迁移： {Migrations}", migrations.Length,
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
        catch (Exception e)
        {
            _logger.LogError(e, "EF 初始化异常");
        }
    }
}
