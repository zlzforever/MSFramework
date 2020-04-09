using System;
using System.Linq;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using MSFramework.Common;
using MSFramework.Extensions;

namespace MSFramework.Ef
{
	public class EntityFrameworkInitializer : Initializer
	{
		public override int Order => int.MinValue;

		public override void Initialize(IServiceProvider serviceProvider)
		{
			var dbContextFactory = serviceProvider.GetRequiredService<DbContextFactory>();
			var entityFrameworkOptionsStore = serviceProvider.GetRequiredService<EntityFrameworkOptionsStore>();
			foreach (var option in entityFrameworkOptionsStore.GetAllOptions())
			{
				var useTrans = option.UseTransaction;
				if (option.AutoMigrationEnabled)
				{
					option.UseTransaction = false;
					var dbContext = dbContextFactory.Create(option);
					if (dbContext == null)
					{
						continue;
					}

					if (dbContext.Database.ProviderName == "Microsoft.EntityFrameworkCore.InMemory") continue;

					var migrations = dbContext.Database.GetPendingMigrations().ToArray();
					if (migrations.Length > 0)
					{
						dbContext.Database.Migrate();
						ILogger logger = dbContext.GetService<ILoggerFactory>()
							.CreateLogger<EntityFrameworkInitializer>();
						logger.LogInformation($"已提交{migrations.Length}条挂起的迁移记录：{migrations.ExpandAndToString()}");
					}

					option.UseTransaction = useTrans;
				}
			}
		}
	}
}