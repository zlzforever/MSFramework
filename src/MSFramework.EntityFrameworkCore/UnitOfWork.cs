using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using MSFramework.Domain;
using MSFramework.EventSouring;

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
			var events = _dbContextProvider.GetAllDbContexts().SelectMany(x => x.GetEventSouringDomainEventsAsync())
				.ToArray();
			if (events.Length > 0)
			{
				// 保存直接提交，如果提交失败，则数据落库也失败
				_dbContextProvider.GetEventStore().AddEventAsync(events).GetAwaiter().GetResult();
			}

			foreach (var dbContext in _dbContextProvider.GetAllDbContexts())
			{
				dbContext.SaveChanges();
			}
		}

		public async Task CommitAsync()
		{
			var events = _dbContextProvider.GetAllDbContexts().SelectMany(x => x.GetEventSouringDomainEventsAsync())
				.ToArray();
			if (events.Length > 0)
			{
				await _dbContextProvider.GetEventStore().AddEventAsync(events);
			}

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