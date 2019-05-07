using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using MSFramework.Domain;

namespace MSFramework.EntityFrameworkCore
{
	public class UnitOfWork : IUnitOfWork
	{
		private readonly DbContextFactory _dbContextProvider;
		private readonly IHttpContextAccessor _accessor;

		public UnitOfWork(DbContextFactory dbContextProvider, IHttpContextAccessor accessor)
		{
			_dbContextProvider = dbContextProvider;
			_accessor = accessor;
		}

		public void Commit()
		{
			var dbContexts = _dbContextProvider.GetAllDbContexts();
			var events = dbContexts.SelectMany(x => x.GetEventSouringEventsAsync())
				.ToArray();

			if (events.Length > 0)
			{
				var userId = _accessor.HttpContext.User.FindFirst("sub")?.Value;
				var userName = _accessor.HttpContext.User.FindFirst("name")?.Value;

				foreach (var @event in events)
				{
					@event.Creator = userName;
					@event.CreatorId = userId;
				}

				// 保存直接提交，如果提交失败，则数据落库也失败
				var eventStore = _dbContextProvider.GetEventStore();
				eventStore?.AddEventsAsync(events).GetAwaiter().GetResult();
			}

			foreach (var dbContext in dbContexts)
			{
				dbContext.SaveChanges();
			}
		}

		public async Task CommitAsync()
		{
			var dbContexts = _dbContextProvider.GetAllDbContexts();
			var events = dbContexts.SelectMany(x => x.GetEventSouringEventsAsync())
				.ToArray();

			if (events.Length > 0)
			{
				var userId = _accessor.HttpContext.User.FindFirst("sub")?.Value;
				var userName = _accessor.HttpContext.User.FindFirst("name")?.Value;

				foreach (var @event in events)
				{
					@event.Creator = userName;
					@event.CreatorId = userId;
				}

				var eventStore = _dbContextProvider.GetEventStore();
				if (eventStore != null)
				{
					await eventStore.AddEventsAsync(events);
				}
			}

			foreach (var dbContext in dbContexts)
			{
				await dbContext.SaveChangesAsync();
			}
		}
	}
}