using System;
using System.Linq;
using System.Threading.Tasks;
using MicroserviceFramework.Extensions;
using MicroserviceFramework.Initializers;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace MicroserviceFramework.Ef.Initializers
{
	public class EntityFrameworkInitializer : Initializer, INonAutomaticInitializer
	{
		public override int Order => int.MinValue;

		public override async Task InitializeAsync(IServiceProvider serviceProvider)
		{
			var entityFrameworkOptionsStore = serviceProvider.GetRequiredService<EntityFrameworkOptionsConfiguration>();
			foreach (var option in entityFrameworkOptionsStore.GetAllOptions())
			{
				if (option.AutoMigrationEnabled)
				{
					var dbContext = (DbContextBase) serviceProvider.GetRequiredService(option.DbContextType);
					if (dbContext == null)
					{
						continue;
					}

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