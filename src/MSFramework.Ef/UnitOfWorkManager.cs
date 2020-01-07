using System;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using MSFramework.Domain;

namespace MSFramework.Ef
{
	public class UnitOfWorkManager : IUnitOfWorkManager
	{
		private readonly DbContextFactory _dbContextFactory;
		private readonly ILogger _logger;

		public UnitOfWorkManager(DbContextFactory dbContextFactory, ILogger<UnitOfWorkManager> logger)
		{
			_dbContextFactory = dbContextFactory;
			_logger = logger;
		}

		public async Task CommitAsync()
		{
			foreach (var dbContext in _dbContextFactory.GetAllDbContexts())
			{
				await dbContext.CommitAsync();
			}

			_logger.LogInformation("DbContexts committed");
		}
	}
}