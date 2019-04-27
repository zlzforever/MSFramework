using System;
using System.Runtime.Serialization.Formatters.Binary;
using Microsoft.Extensions.Caching.Memory;
using MSFramework.Domain.Event;
using MSFramework.EventSouring;
using MSFramework.Snapshot;

namespace MSFramework.Domain
{
	public class AggregateRootMemoryCache : IAggregateRootCache
	{
		private readonly IMemoryCache _cache;
		private readonly IEventStore _eventStore;
		private readonly ISnapshotStore _snapshotStore;

		public AggregateRootMemoryCache(IMemoryCache cache, ISnapshotStore snapshotStore,
			IEventStore eventStore)
		{
			_cache = cache;
			_eventStore = eventStore;
			_snapshotStore = snapshotStore;
		}

		public void Set<TAggregateRoot, TAggregateRootId>(TAggregateRoot aggregateRoot, int? version = null)
			where TAggregateRoot : AggregateRootBase<TAggregateRoot, TAggregateRootId>
			where TAggregateRootId : IEquatable<TAggregateRootId>
		{
			var memory = new System.IO.MemoryStream();
			BinaryFormatter formatter = new BinaryFormatter();
			formatter.Serialize(memory, aggregateRoot);
			throw new NotImplementedException();
		}

		public TAggregateRoot Get<TAggregateRoot, TAggregateRootId>(TAggregateRootId aggregateRootId)
			where TAggregateRoot : AggregateRootBase<TAggregateRoot, TAggregateRootId>
			where TAggregateRootId : IEquatable<TAggregateRootId>
		{
			var bytes = _cache.Get<byte[]>(aggregateRootId);
			if (bytes == null)
			{
				
			}
			else
			{
				var memory = new System.IO.MemoryStream(bytes);
				BinaryFormatter formatter = new BinaryFormatter();
				var aggregateRoot = (IAggregateRoot) formatter.Deserialize(memory);
				memory.Close();
				
				var @event = _eventStore.GetLastEvent(aggregateRoot.GetId());
			}

			if (eventStore == null)
			{
				return aggregate;
			}
			else
			{
				var idAsString = id.ToString();
				if (aggregate == null)
				{
					var @event = eventStore.GetLastEvent(idAsString);
					if (@event.EventType != typeof(DeletedEvent<TAggregateRoot, TAggregateRootId>).FullName)
					{
						aggregate = AggregateRootFactory.CreateAggregate<TAggregateRoot>();
						var events = eventStore.GetEvents(idAsString, 0);
						aggregate.LoadFromHistory(events.Select(e => e.ToAggregateEvent()).ToArray());
						Aggregates.Add(aggregate);
						return aggregate;
					}
					else
					{
						return null;
					}
				}
				else
				{
					var events = eventStore.GetEvents(idAsString, aggregate.Version);
					if (events.Any() && events.First().Version != aggregate.Version + 1)
					{
						// TODO: data is dirty
						throw new MSFrameworkException(
							$"Entity {typeof(TAggregateRoot)} Id {id} is not match in event store and can't auto rebuild");
					}
					else
					{
						aggregate.LoadFromHistory(events.Select(e => e.ToAggregateEvent()).ToArray());
						return aggregate;
					}
				}
			}
		}
	}
}