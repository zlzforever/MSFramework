using System.Threading.Tasks;
using MSFramework.Domain;

namespace MSFramework.Ef
{
	public class UnitOfWorkManager : IUnitOfWorkManager
	{
		private readonly DbContextFactory _dbContextFactory;

		public UnitOfWorkManager(DbContextFactory dbContextFactory)
		{
			_dbContextFactory = dbContextFactory;
		}

		public void Commit()
		{
			foreach (var dbContext in _dbContextFactory.GetAllDbContexts())
			{
				dbContext.Commit();
				dbContext.Dispose();
			}
		}

		public async Task CommitAsync()
		{
			foreach (var dbContext in _dbContextFactory.GetAllDbContexts())
			{
				await dbContext.CommitAsync();
				await dbContext.DisposeAsync();
			}
		}
	}
}