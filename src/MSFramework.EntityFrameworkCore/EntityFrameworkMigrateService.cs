using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using MSFramework.Collections.Generic;
using MSFramework.Domain;

namespace MSFramework.EntityFrameworkCore
{
	public class EntityFrameworkMigrateService : IHostedService
	{
		private readonly IServiceProvider _serviceProvider;

		public EntityFrameworkMigrateService(IServiceProvider serviceProvider)
		{
			_serviceProvider = serviceProvider;
		}

		public Task StartAsync(CancellationToken cancellationToken)
		{
			using (var scope = _serviceProvider.CreateScope())
			{
				var dbContextFactory = scope.ServiceProvider.GetRequiredService<DbContextFactory>();
				foreach (var kv in EntityFrameworkOptions.EntityFrameworkOptionDict)
				{
					if (kv.Value.AutoMigrationEnabled)
					{
						var dbContext = dbContextFactory.Create(kv.Value);
						if (dbContext == null)
						{
							continue;
						}

						if (dbContext.Database.ProviderName == "Microsoft.EntityFrameworkCore.InMemory") continue;

						dbContext.Database.EnsureCreated();
						string[] migrations = dbContext.Database.GetPendingMigrations().ToArray();
						if (migrations.Length > 0)
						{
							dbContext.Database.Migrate();
							ILogger logger = dbContext.GetService<ILoggerFactory>()
								.CreateLogger("Slug.Entity.DbContextExtensions");
							logger.LogInformation($"已提交{migrations.Length}条挂起的迁移记录：{migrations.ExpandAndToString()}");
						}
					}
				}
			}

			return Task.CompletedTask;
		}

		public Task StopAsync(CancellationToken cancellationToken)
		{
			return Task.CompletedTask;
		}
	}
}