using System.Threading;
using System.Threading.Tasks;
using MSFramework.Domain;

namespace MSFramework.EntityFrameworkCore
{
	public class UnitOfWork : IUnitOfWork
	{
		private readonly DbContextFactory _dbContextProvider;

		public UnitOfWork(DbContextFactory dbContextProvider)
		{
			_dbContextProvider = dbContextProvider;
		}

		public void Commit()
		{
			foreach (var dbContext in _dbContextProvider.GetAllDbContexts())
			{
				dbContext.SaveChanges();
			}
		}

		public async Task CommitAsync()
		{
			foreach (var dbContext in _dbContextProvider.GetAllDbContexts())
			{
				await dbContext.SaveChangesAsync();
			}
		}

		public void BeginOrUseTransaction()
		{
			// TODO
			 
		}

		public Task BeginOrUseTransactionAsync(CancellationToken cancellationToken = default)
		{
			// TODO
			return Task.CompletedTask;
		}
	}
}