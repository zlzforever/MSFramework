using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using MSFramework.EntityFrameworkCore;

namespace MSFramework.EventSouring.EntityFrameworkCore
{
	public class EfEventStore : IEventStore
	{
		private readonly DbSet<EventHistory> _table;
		private readonly DbContext _dbContext;

		public EfEventStore(DbContextFactory factory)
		{
			_dbContext = factory.GetDbContext<EventHistory>();
			_table = _dbContext.Set<EventHistory>();
		}

		public async Task<EventHistory[]> GetEventsAsync(Guid aggregateId, long from)
		{
			return await _table.Where(x => x.Version > from && x.AggregateId == aggregateId).ToArrayAsync();
		}

		public EventHistory[] GetEvents(Guid aggregateId, long from)
		{
			return _table.Where(x => x.Version > from && x.AggregateId == aggregateId).ToArray();
		}

		public async Task AddEventAsync(params EventHistory[] events)
		{
			foreach (var @event in events)
			{
				await _table.AddAsync(@event);
			}

			await _dbContext.SaveChangesAsync();
		}

		public async Task<EventHistory> GetLastEventAsync(Guid aggregateId)
		{
			return await _table.Where(x => x.AggregateId == aggregateId).OrderByDescending(x => x.Version)
				.FirstOrDefaultAsync();
		}

		public EventHistory GetLastEvent(Guid aggregateId)
		{
			return _table.Where(x => x.AggregateId == aggregateId).OrderByDescending(x => x.Version)
				.FirstOrDefault();
		}
	}
}