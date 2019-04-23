using MSFramework.Domain;
using MSFramework.Domain.Repository;
using System;
using System.Linq;
using System.Threading.Tasks;
using MSFramework.EventSouring;

namespace MSFramework.EntityFrameworkCore.Repository
{
	public class EFAggrateRepository<TAggrate, TKey> : EfRepository<TAggrate, TKey>, IAggregateRepository<TAggrate, TKey>
		where TAggrate : AggregateRootBase<TKey>
		where TKey : IEquatable<TKey>
	{
		private readonly IEventStore _eventStore;

		public EFAggrateRepository(DbContextFactory dbContextFactory, IEventStore eventStore) : base(dbContextFactory)
		{
			_eventStore = eventStore;
		}

		public override async Task<TAggrate> GetAsync(TKey id)
		{
			var aggregate = await base.GetAsync(id);
			if (aggregate != null)
			{
				var events = await _eventStore.GetEventsAsync(aggregate.IdAsString(), aggregate.Version);
				aggregate.LoadFromHistory(events.Select(a => a.ToAggregateEvent()));
			}

			return aggregate;
		}
	}
}
