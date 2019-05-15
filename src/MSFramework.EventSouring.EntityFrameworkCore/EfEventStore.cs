using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using MSFramework.EntityFrameworkCore;

namespace MSFramework.EventSouring.EntityFrameworkCore
{
	public class EfEventStore : IEventStore
	{
		private readonly DbSet<StoredEvent> _table;
		private readonly DbContext _dbContext;

		public EfEventStore(DbContextFactory factory)
		{
			_dbContext = factory.GetDbContext<StoredEvent>();
			_table = _dbContext.Set<StoredEvent>();
		}

		public async Task<StoredEvent[]> GetEventsAsync(Guid aggregateRootId, long from)
		{
			return await _table.Where(x => x.Version > from && x.AggregateRootId == aggregateRootId).ToArrayAsync();
		}

		public StoredEvent[] GetEvents(Guid aggregateRootId, long from)
		{
			return _table.Where(x => x.Version > from && x.AggregateRootId == aggregateRootId).ToArray();
		}

		public async Task AddEventsAsync(params StoredEvent[] events)
		{
			foreach (var @event in events)
			{
				await _table.AddAsync(@event);
			}

			await _dbContext.SaveChangesAsync();
		}

		public void AddEvents(params StoredEvent[] events)
		{
			foreach (var @event in events)
			{
				_table.Add(@event);
			}

			_dbContext.SaveChanges();
		}

		public async Task<StoredEvent> GetLastEventAsync(Guid aggregateRootId)
		{
			return await _table.Where(x => x.AggregateRootId == aggregateRootId).OrderByDescending(x => x.Version)
				.FirstOrDefaultAsync();
		}

		public StoredEvent GetLastEvent(Guid aggregateRootId)
		{
			return _table.Where(x => x.AggregateRootId == aggregateRootId).OrderByDescending(x => x.Version)
				.FirstOrDefault();
		}
	}
}