using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using MSFramework.Domain.Repository;
using MSFramework.EntityFrameworkCore.Repository;
using MSFramework.EventBus;
using MSFramework.EventSouring;
using Newtonsoft.Json;

namespace MSFramework.EntityFrameworkCore
{
	internal class EventSourceEntryConfiguration : EntityTypeConfigurationBase<EventHistory>
	{
		public override void Configure(EntityTypeBuilder<EventHistory> builder)
		{
			builder.ToTable("EventSource");
			builder.Property("EventType");
			builder.Property("Event");
			builder.Property("AggregateId");
			builder.Property("Version");
			builder.Property("CreationTime");
			builder.HasKey(x => x.Id);
			builder.HasIndex(x => new {x.AggregateId, x.Version}).IsUnique();
			builder.HasIndex(x => x.CreationTime);
		}
	}
	
	public class EfEventStore : IEventStore
	{
		private readonly IEfRepository<EventHistory, long> _repository;

		public EfEventStore(IEfRepository<EventHistory, long> repository)
		{
			_repository = repository;
		}

		public async Task<IEnumerable<EventHistory>> GetEventsAsync(string aggregateId, long @from)
		{
			return await _repository.GetAllListAsync(x => x.Version > from && x.AggregateId == aggregateId);
		}

		public async Task AddEventAsync(params EventHistory[] events)
		{
			foreach (var @event in events)
			{
				await _repository.InsertAsync(@event);
			}
		}
	}
}