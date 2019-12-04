using System;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using MSFramework.Domain;

namespace MSFramework.EntityFrameworkCore
{
	public class UnitOfWorkManager : IUnitOfWorkManager
	{
		private readonly DbContextFactory _dbContextFactory;
		private readonly IServiceProvider _serviceProvider;

		public UnitOfWorkManager(DbContextFactory dbContextFactory, IServiceProvider serviceProvider)
		{
			_dbContextFactory = dbContextFactory;
			_serviceProvider = serviceProvider;
		}

		public async Task CommitAsync()
		{
			ILogger logger = null;
			if (!_serviceProvider.GetRequiredService<IHostEnvironment>().IsProduction())
			{
				logger = _serviceProvider.GetRequiredService<ILogger<DbContextFactory>>();
			}

			foreach (var dbContext in _dbContextFactory.GetAllDbContexts())
			{
				await dbContext.CommitAsync();
				logger?.LogInformation("DbContexts autoCommitted");
			}
		}
	}
}