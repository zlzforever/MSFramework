using System;
using System.Linq;
using System.Threading.Tasks;
using MicroserviceFramework.Extensions;
using MicroserviceFramework.Initializer;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace MicroserviceFramework.Ef.Initializer
{
	[IgnoreRegister]
	public class EntityFrameworkInitializer : InitializerBase
	{
		public override int Order => int.MinValue;

		public override async Task InitializeAsync(IServiceProvider serviceProvider)
		{
			var dbContextConfigurationCollection =
				serviceProvider.GetRequiredService<IOptions<DbContextConfigurationCollection>>().Value;

			if (dbContextConfigurationCollection.Count == 0)
			{
				throw new MicroserviceFrameworkException("未能找到数据上下文配置");
			}

			var repeated = dbContextConfigurationCollection
				.GroupBy(m => m.DbContextType)
				.FirstOrDefault(m => m.Count() > 1);
			if (repeated != null)
			{
				throw new MicroserviceFrameworkException(
					$"数据上下文配置中存在多个配置节点指向同一个上下文类型：{repeated.First().DbContextTypeName}");
			}

			foreach (var option in dbContextConfigurationCollection)
			{
				if (option.AutoMigrationEnabled)
				{
					var dbContext = (DbContextBase) serviceProvider.GetRequiredService(option.DbContextType);

					if (dbContext.Database.ProviderName == "Microsoft.EntityFrameworkCore.InMemory")
					{
						continue;
					}

					var migrations = (await dbContext.Database.GetPendingMigrationsAsync()).ToArray();
					if (migrations.Length > 0)
					{
						await dbContext.Database.MigrateAsync();
						ILogger logger = dbContext.GetService<ILoggerFactory>()
							.CreateLogger<EntityFrameworkInitializer>();
						logger.LogInformation($"已提交 {migrations.Length} 条挂起的迁移记录：{migrations.ExpandAndToString()}");
					}
				}
			}
		}
	}
}