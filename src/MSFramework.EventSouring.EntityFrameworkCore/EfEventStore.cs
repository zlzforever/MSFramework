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

		public async Task<EventHistory[]> GetEventsAsync(string aggregateId, long from)
		{
			return await _table.Where(x => x.Version > from && x.AggregateRootId == aggregateId).ToArrayAsync();
		}

		public EventHistory[] GetEvents(string aggregateId, long from)
		{
			return _table.Where(x => x.Version > from && x.AggregateRootId == aggregateId).ToArray();
		}

		public async Task AddEventsAsync(params EventHistory[] events)
		{
			foreach (var @event in events)
			{
				await _table.AddAsync(@event);
			}

			await _dbContext.SaveChangesAsync();
		}

		public void AddEvents(params EventHistory[] events)
		{
			foreach (var @event in events)
			{
				_table.Add(@event);
			}

			_dbContext.SaveChanges();
		}

		public async Task<EventHistory> GetLastEventAsync(string aggregateId)
		{
			return await _table.Where(x => x.AggregateRootId == aggregateId).OrderByDescending(x => x.Version)
				.FirstOrDefaultAsync();
		}

		public EventHistory GetLastEvent(string aggregateId)
		{
			return _table.Where(x => x.AggregateRootId == aggregateId).OrderByDescending(x => x.Version)
				.FirstOrDefault();
		}
	}
}