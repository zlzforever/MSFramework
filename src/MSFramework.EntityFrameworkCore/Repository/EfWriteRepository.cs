using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;
using Castle.Core.Internal;
using Microsoft.EntityFrameworkCore;
using MSFramework.Domain;
using MSFramework.Domain.Event;
using MSFramework.Domain.Repository;
using Z.EntityFramework.Plus;

namespace MSFramework.EntityFrameworkCore.Repository
{
	/// <summary>
	/// 实体数据存储操作类
	/// </summary>
	/// <typeparam name="TAggregateRoot">实体类型</typeparam>
	public abstract class EfWriteRepository<TAggregateRoot> :
		IEfWriteRepository<TAggregateRoot>
		where TAggregateRoot : AggregateRootBase
	{
		protected DbContextFactory DbContextFactory { get; }

		protected DbContext DbContext { get; }

		protected DbSet<TAggregateRoot> Table { get; }

		public EfWriteRepository(DbContextFactory dbContextFactory)
		{
			DbContextFactory = dbContextFactory;
			DbContext = dbContextFactory.GetDbContext<TAggregateRoot>();
			Table = DbContext.Set<TAggregateRoot>();
		}

		public virtual TAggregateRoot Insert(TAggregateRoot entity)
		{
			Table.Add(entity);
			return entity;
		}

		public virtual async Task<TAggregateRoot> InsertAsync(TAggregateRoot entity)
		{
			await Table.AddAsync(entity);
			return entity;
		}

		public virtual TAggregateRoot Update(TAggregateRoot entity)
		{
			Table.Update(entity);
			return entity;
		}

		public virtual Task<TAggregateRoot> UpdateAsync(TAggregateRoot entity)
		{
			return Task.FromResult(Update(entity));
		}

		public virtual void Delete(TAggregateRoot entity)
		{
			Table.Remove(entity);
		}

		public virtual Task DeleteAsync(TAggregateRoot entity)
		{
			Delete(entity);
			return Task.CompletedTask;
		}

		public virtual void Delete(Guid id)
		{
			var entity = GetFromChangeTrackerOrNull(id);
			if (entity != null)
			{
				Delete(entity);
				return;
			}

			entity = Table.FirstOrDefault(x => x.Id == id);
			if (entity != null)
			{
				Delete(entity);
			}
		}

		public virtual Task DeleteAsync(Guid id)
		{
			Delete(id);
			return Task.FromResult(0);
		}

		public virtual TAggregateRoot Get(Guid id)
		{
			var aggregate = Table.FirstOrDefault(x => x.Id == id);
			if (aggregate == null)
			{
				var eventStore = DbContextFactory.GetEventStore();
				if (eventStore == null)
				{
					return null;
				}
				else
				{
					var @event = eventStore.GetLastEventAsync(id).Result;
					if (@event.EventType != DeletedEvent.Type.FullName)
					{
						// TODO: should rebuild
						throw new MSFrameworkException(
							$"Entity {typeof(TAggregateRoot)} Id {id} is not exits, but the last aggregate event in event store is not DeletedEvent");
					}
					else
					{
						return null;
					}
				}
			}
			else
			{
				var eventStore = DbContextFactory.GetEventStore();
				if (eventStore == null)
				{
					return aggregate;
				}
				else
				{
					var events = eventStore.GetEventsAsync(aggregate.Id, aggregate.Version).Result;
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

		public virtual async Task<TAggregateRoot> GetAsync(Guid id)
		{
			var aggregate = Table.FirstOrDefault(x => x.Id == id);
			if (aggregate == null)
			{
				var eventStore = DbContextFactory.GetEventStore();
				if (eventStore == null)
				{
					return null;
				}
				else
				{
					var @event = await eventStore.GetLastEventAsync(id);
					if (@event.EventType != DeletedEvent.Type.FullName)
					{
						// TODO: should rebuild
						throw new MSFrameworkException(
							$"Entity {typeof(TAggregateRoot)} Id {id} is not exits, but the last aggregate event in event store is not DeletedEvent");
					}
					else
					{
						return null;
					}
				}
			}
			else
			{
				var eventStore = DbContextFactory.GetEventStore();
				if (eventStore == null)
				{
					return aggregate;
				}
				else
				{
					var events = await eventStore.GetEventsAsync(aggregate.Id, aggregate.Version);
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

		private TAggregateRoot GetFromChangeTrackerOrNull(Guid id)
		{
			var entry = DbContext.ChangeTracker.Entries()
				.FirstOrDefault(ent => ent.Entity is TAggregateRoot aggregate && aggregate.Id == id);

			return entry?.Entity as TAggregateRoot;
		}
	}
}