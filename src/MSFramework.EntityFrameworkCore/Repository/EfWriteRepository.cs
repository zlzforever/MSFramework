using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using MSFramework.Domain;
using MSFramework.Domain.Event;
using MSFramework.Domain.Repository;

namespace MSFramework.EntityFrameworkCore.Repository
{
	/// <summary>
	/// 实体数据存储操作类
	/// </summary>
	/// <typeparam name="TAggregateRoot">实体类型</typeparam>
	/// <typeparam name="TAggregateRootId"></typeparam>
	public abstract class EfWriteRepository<TAggregateRoot, TAggregateRootId> :
		IWriteRepository<TAggregateRoot, TAggregateRootId>
		where TAggregateRoot : AggregateRootBase<TAggregateRoot, TAggregateRootId>
		where TAggregateRootId : IEquatable<TAggregateRootId>
	{
		protected DbContextFactory DbContextFactory { get; }

		protected DbContext DbContext { get; }

		protected DbSet<TAggregateRoot> Aggregates { get; }

		public EfWriteRepository(DbContextFactory dbContextFactory)
		{
			DbContextFactory = dbContextFactory;
			DbContext = dbContextFactory.GetDbContext<TAggregateRoot>();
			Aggregates = DbContext.Set<TAggregateRoot>();
		}

		public virtual TAggregateRoot Insert(TAggregateRoot entity)
		{
			Aggregates.Add(entity);
			return entity;
		}

		public virtual async Task<TAggregateRoot> InsertAsync(TAggregateRoot entity)
		{
			await Aggregates.AddAsync(entity);
			return entity;
		}

		public virtual TAggregateRoot Update(TAggregateRoot entity)
		{
			Aggregates.Update(entity);
			return entity;
		}

		public virtual Task<TAggregateRoot> UpdateAsync(TAggregateRoot entity)
		{
			return Task.FromResult(Update(entity));
		}

		public virtual void Delete(TAggregateRoot entity)
		{
			Aggregates.Remove(entity);
		}

		public virtual Task DeleteAsync(TAggregateRoot entity)
		{
			Delete(entity);
			return Task.CompletedTask;
		}

		public virtual void Delete(TAggregateRootId id)
		{
			var entity = GetFromChangeTrackerOrNull(id);
			if (entity != null)
			{
				Delete(entity);
				return;
			}

			entity = Aggregates.FirstOrDefault(x => x.Id.Equals(id));
			if (entity != null)
			{
				Delete(entity);
			}
		}

		public virtual Task DeleteAsync(TAggregateRootId id)
		{
			Delete(id);
			return Task.FromResult(0);
		}

		public virtual TAggregateRoot Get(TAggregateRootId id)
		{
			var eventStore = DbContextFactory.GetEventStore();
			var aggregate = Aggregates.FirstOrDefault(x => x.Id.Equals(id));
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

		public virtual async Task<TAggregateRoot> GetAsync(TAggregateRootId id)
		{
			var eventStore = DbContextFactory.GetEventStore();
			var aggregate = Aggregates.FirstOrDefault(x => x.Id.Equals(id));
			if (eventStore == null)
			{
				return aggregate;
			}
			else
			{
				var idAsString = id.ToString();
				if (aggregate == null)
				{
					var @event = await eventStore.GetLastEventAsync(idAsString);
					if (@event.EventType != typeof(DeletedEvent<TAggregateRoot, TAggregateRootId>).FullName)
					{
						aggregate = AggregateRootFactory.CreateAggregate<TAggregateRoot>();
						var events = await eventStore.GetEventsAsync(idAsString, 0);
						aggregate.LoadFromHistory(events.Select(e => e.ToAggregateEvent()).ToArray());
						await Aggregates.AddAsync(aggregate);
						return aggregate;
					}
					else
					{
						return null;
					}
				}
				else
				{
					var events = await eventStore.GetEventsAsync(idAsString, aggregate.Version);
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

		private TAggregateRoot GetFromChangeTrackerOrNull(TAggregateRootId id)
		{
			var entry = DbContext.ChangeTracker.Entries()
				.FirstOrDefault(ent => ent.Entity is TAggregateRoot aggregate && aggregate.Id.Equals(id));

			return entry?.Entity as TAggregateRoot;
		}
	}
}