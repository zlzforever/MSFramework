using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using MSFramework.CQRS.EventSouring;
using MSFramework.Domain.Repository;
using MSFramework.EntityFrameworkCore.Repository;
using MSFramework.EventBus;
using Newtonsoft.Json;

namespace MSFramework.EntityFrameworkCore
{
	public class EfEventStore : IEventStore
	{
		private readonly IEfRepository<EventSourceEntry, long> _repository;

		public EfEventStore(IEfRepository<EventSourceEntry, long> repository)
		{
			_repository = repository;
		}

		public async Task<IEnumerable<Event>> GetEventsAsync(string aggregateId, long @from)
		{
			var entries = await _repository.GetAllListAsync(x => x.Version > from && x.AggregateId == aggregateId);

			return entries.Select(x => (Event) JsonConvert.DeserializeObject(x.Event, Type.GetType(x.EventType)));
		}

		public async Task AddEventAsync(params EventSourceEntry[] events)
		{
			foreach (var @event in events)
			{
				await _repository.InsertAsync(@event);
			}
		}
	}
}