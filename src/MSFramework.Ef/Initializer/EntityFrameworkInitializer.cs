using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using MicroserviceFramework.Extensions;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace MicroserviceFramework.Ef.Initializer
{
	public class EntityFrameworkInitializer : InitializerBase
	{
		private readonly IServiceProvider _serviceProvider;
		private readonly ILogger<EntityFrameworkInitializer> _logger;

		public EntityFrameworkInitializer(IServiceProvider serviceProvider, ILogger<EntityFrameworkInitializer> logger)
		{
			_serviceProvider = serviceProvider;
			_logger = logger;
		}

		public override async Task StartAsync(CancellationToken cancellationToken)
		{
			try
			{
				using var scope = _serviceProvider.CreateScope();

				var dbContextConfigurationCollection =
					scope.ServiceProvider.GetRequiredService<IOptions<DbContextConfigurationCollection>>().Value;

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
						$"数据上下文配置中存在多个配置节点指向同一个上下文类型： {repeated.First().DbContextTypeName}");
				}

				foreach (var option in dbContextConfigurationCollection)
				{
					if (option.AutoMigrationEnabled)
					{
						var dbContext = (DbContextBase)scope.ServiceProvider.GetRequiredService(option.DbContextType);

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
							ILogger logger = dbContext.GetService<ILoggerFactory>()
								.CreateLogger<EntityFrameworkInitializer>();
							logger.LogInformation(
								$"已提交 {migrations.Length} 条挂起的迁移记录： {migrations.ExpandAndToString()}");
						}
					}
				}
			}
			catch (Exception e)
			{
				_logger.LogError(e.ToString());
			}
		}
	}
}