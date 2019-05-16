using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MSFramework.Domain.Exception;
using MSFramework.EventBus;
using MSFramework.EventSouring;

namespace MSFramework.Domain.Repository
{
	public class EventStoreRepository : IRepository
	{
		private readonly IEventStore _eventStore;
		private readonly IEventBus _eventBus;

		protected EventStoreRepository(IEventStore es, IEventBus eventBus)
		{
			_eventStore = es;
			_eventBus = eventBus;
		}

		public virtual async Task AppendAsync<TAggregateRoot>(IEnumerable<TAggregateRoot> aggregateRoots)
			where TAggregateRoot : AggregateRootBase
		{
			var events = new List<Event>();
			var aggregateRootArray = aggregateRoots as TAggregateRoot[] ?? aggregateRoots.ToArray();
			foreach (var aggregateRoot in aggregateRootArray)
			{
				events.AddRange(aggregateRoot.GetUncommittedChanges());
			}

			// 启用事务，保证在一个生命周期内所有的事件的提交是原子性的
			await _eventStore.AddEventsAsync(events.Select(x => new StoredEvent(x)).ToArray());
			// 如何保证所有事件消费成功、或者如果消费失败如何处理
			foreach (var @event in events)
			{
				await _eventBus.PublishAsync(@event);
			}

			foreach (var aggregateRoot in aggregateRootArray)
			{
				aggregateRoot.ClearChanges();
			}
		}

		public virtual async Task<TAggregateRoot> GetAsync<TAggregateRoot>(Guid aggregateRootId,
			int? expectedVersion = null)
			where TAggregateRoot : AggregateRootBase
		{
			var aggregate = AggregateRootFactory.CreateAggregate<TAggregateRoot>();

			var events = (await _eventStore.GetEventsAsync(aggregateRootId, -1)).Select(x => x.ToEvent())
				.ToArray();
			if (events.Length == 0)
			{
				throw new MSFrameworkException($"AggregateRoot not found: {aggregateRootId}");
			}

			aggregate.LoadFromHistory(events);
			return aggregate;
		}
	}
}