using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace MicroserviceFramework.Ef.Initializer;

public class EntityFrameworkInitializer(IServiceProvider serviceProvider, ILogger<EntityFrameworkInitializer> logger)
    : IInitializerBase
{
    public async Task StartAsync(CancellationToken cancellationToken)
    {
        try
        {
            logger.LogInformation("开始 EF 初始化...");
            using var scope = serviceProvider.CreateScope();

            var dbContextConfigurationCollection =
                scope.ServiceProvider.GetRequiredService<IOptions<DbContextSettingsList>>().Value;

            if (dbContextConfigurationCollection.Count == 0)
            {
                throw new MicroserviceFrameworkException("未能找到数据上下文配置");
            }

            var repeated = dbContextConfigurationCollection
                .GroupBy(m => m.GetDbContextType())
                .FirstOrDefault(m => m.Count() > 1);
            if (repeated != null)
            {
                throw new MicroserviceFrameworkException(
                    $"数据上下文配置中存在多个配置节点指向同一个上下文类型： {repeated.First().DbContextTypeName}");
            }

            foreach (var option in dbContextConfigurationCollection)
            {
                if (option.AutoMigrationEnabled)
                {
                    logger.LogInformation("数据库上下文 {DbContextTypeName} 中开启了数据库自动迁移", option.DbContextTypeName);

                    var dbContext = (DbContextBase)scope.ServiceProvider.GetRequiredService(option.GetDbContextType());

                    if (dbContext.Database.ProviderName == "Microsoft.EntityFrameworkCore.InMemory")
                    {
                        continue;
                    }

                    ILogger logger1 = dbContext.GetService<ILoggerFactory>()
                        .CreateLogger<EntityFrameworkInitializer>();

                    // var appliedMigrations =
                    //     (await dbContext.Database.GetAppliedMigrationsAsync(cancellationToken: cancellationToken))
                    //     .ToList();
                    // logger.LogInformation("Applied {AppliedMigrationsCount} migrations： {AppliedMigrations}",
                    //     appliedMigrations.Count, string.Join(", ", appliedMigrations));

                    var migrations =
                        (await dbContext.Database.GetPendingMigrationsAsync(cancellationToken: cancellationToken))
                        .ToArray();
                    if (migrations.Length > 0)
                    {
                        await dbContext.Database.MigrateAsync(cancellationToken: cancellationToken);

                        logger1.LogInformation("执行了 {MigrationsCount} 个数据库迁移： {Migrations}", migrations.Length,
                            string.Join(", ", migrations));
                    }
                    else
                    {
                        logger.LogInformation("数据库上下文 {DbContextTypeName} 中没有挂起的迁移",
                            option.DbContextTypeName);
                    }
                }
                else
                {
                    logger.LogInformation("数据库上下文 {DbContextTypeName} 禁用了自动迁移", option.DbContextTypeName);
                }
            }

            logger.LogInformation("EF 初始化完成");
        }
        catch (Exception e)
        {
            logger.LogError(e, "EF 初始化异常");
        }
    }

    public int Order => int.MinValue;
}
